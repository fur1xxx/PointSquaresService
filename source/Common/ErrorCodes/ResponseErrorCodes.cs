namespace Common.ErrorCodes;

public static class ResponseErrorCodes
{
    public const string BadRequest = "BAD_REQUEST";
    public const string InternalError = "INTERNAL_ERROR";
    public const string CollectionNotFound = "COLLECTION_NOT_FOUND";
    public const string PointAlreadyExists = "POINT_ALREADY_EXISTS";
    public const string PointNotFound = "POINT_NOT_FOUND";
    public const string CollectionFull = "COLLECTION_FULL";
    public const string CollectionBusy = "COLLECTION_BUSY";
}
