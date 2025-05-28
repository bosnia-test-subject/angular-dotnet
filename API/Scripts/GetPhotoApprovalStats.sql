CREATE PROCEDURE GetPhotoApprovalStats
    @CurrentUserId NVARCHAR(450)
AS
BEGIN
    IF EXISTS (
        SELECT 1
        FROM AspNetUserRoles as UR
        JOIN AspNetRoles as R ON UR.RoleId = R.Id
        WHERE UR.UserId = @CurrentUserId AND R.Name = 'Admin'
    )
    BEGIN
        SELECT 
            U.UserName,
            COUNT(CASE WHEN P.IsApproved = 1 THEN 1 END) AS ApprovedPhotos,
            COUNT(CASE WHEN P.IsApproved = 0 THEN 1 END) AS UnapprovedPhotos
        FROM AspNetUsers as U
        LEFT JOIN Photos as P ON U.Id = P.AppUserId
        GROUP BY U.UserName
        ORDER BY U.UserName;
    END
    ELSE
    BEGIN
        RAISERROR('You do not have permission to execute this procedure.', 16, 1);
    END
END;
