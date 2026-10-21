# PointSquaresService

A .NET 10 API that stores collections of 2D integer points in MySQL 8.4 and counts the axis-aligned squares they
form. A square is 4 distinct stored points, sides parallel to the axes, side > 0. A collection holds up to
1,000,000 points; one import sends up to 100,000 rows.

All routes start with `/point-squares/public/v1`.

## Run and test

| Command | Needs | Does |
|---------|-------|------|
| `make run` | Docker with Compose v2, port 8080 free, internet on the first run. No .NET | Starts MySQL, applies the migrations, starts the API |
| `make test` | .NET 10 SDK | Runs the 82 unit tests (no database) |
| `make dev-db` | Docker, .NET 10 SDK | Starts only MySQL on port 3368 and applies the migrations; then run the API from the IDE |
| `make down` | | Removes the containers **and their data** |

Without `make` (e.g. Windows): `docker compose up -d --build --wait`.

- Swagger: http://localhost:8080/point-squares/swagger
- Health check: http://localhost:8080/point-squares/v1/health-check

MySQL has to run in UTC. Both compose files start it with `--default-time-zone=+00:00`.

## How I prioritized the work

I spent about around 10 hours on this.

I did the core first: collections, adding and removing points, and the import, all keeping the squares count
correct. I also put time into keeping the code clean and easy to follow. Then I ran out of time, so three endpoints and functional, API tests
are not there:

- `GET /collections/{id}/squares`, the list of squares. The search itself is written and unit-tested
  (`SquareFinder.FindAll`). What's missing is the endpoint around it, with a `limit` and a time budget so a
  1M-point collection can't keep it busy for minutes.
- `GET /collections/{id}/points`, the area search. One range query on the primary key `(collection_id, x, y)`
  with a `limit`.
- `DELETE /collections/{id}`. Points and then the collection, in one transaction, under the same lock as the
  other writes.

The first two are in the assignment, so they are real gaps.

## The main decision: count on write, list on read

Every add, remove and import updates `squaresCount` in the same transaction as the points. It only checks the
squares that touch the changed points, so reading the count is one `SELECT`. The squares themselves are not stored:
a full 1000 × 1000 grid has 332,833,500 of them, and the list would be found when someone asks for it.

Measured on the running API (`make run`, macOS arm64), not in a benchmark project:

| Case | Time |
|------|------|
| Import 100,000 points into a 1000 × 1000 grid missing its last 100 columns | 6.09 s, 49,783,350 squares added, total 332,833,500 (correct) |
| Import 100,000 random points into 900,000 random points | 2.49 s |
| Add or remove one point in a full 1000 × 1000 grid | 0.02–0.18 s |

## Edge cases I handled

- **Several users on one collection.** Every write locks the collection's row first (`SELECT ... FOR UPDATE`,
  READ COMMITTED), so writes to one collection go one at a time and the counts can't drift. Other collections and
  reads don't wait. A write that waits more than 10 s gets 409 `COLLECTION_BUSY` and can retry. 72 parallel adds
  (36 points, each sent twice) gave exactly 36 × 201 and 36 × 409.
- **An import is one transaction.** It is saved completely or not at all. One that would pass 1,000,000 points is
  rejected as a whole (409 `COLLECTION_FULL`).
- **Messy import data.** Bad rows (missing `x` or `y`, `"5"`, `1.5`, outside the int range, not an object) are
  skipped and listed with their row number and reason. A point repeated in the file is kept once. Points already in
  the collection are not added again, so sending the same file twice adds nothing.
- **A collection is a set.** Adding a point that exists returns 409, removing a missing one returns 404.

## Future improvements

- Functional and API tests on a real MySQL (Testcontainers): the stored count against a full recount, parallel
  writes to one collection, a messy import sent twice.
- Paging instead of a hard `limit`, ideally a cursor on `(x, y)`, so going deep into 1M points stays fast.
- Finish the three missing endpoints: squares list, area search, delete collection.
- More edge cases, for example broken Unicode in an import row (`"\ud800"`): today it returns 500, it should be
  reported as a bad row.

