CREATE PROCEDURE GetUsersWithoutMainPhoto
    @CurrentUserId NVARCHAR(450) 
AS
BEGIN
    IF EXISTS (
        SELECT 1
        FROM AspNetUserRoles ur
        JOIN AspNetRoles r ON ur.RoleId = r.Id
        WHERE ur.UserId = @CurrentUserId AND r.Name = 'Admin'
    )
    BEGIN
        SELECT u.UserName
        FROM AspNetUsers u
        WHERE NOT EXISTS (
            SELECT 1
            FROM Photos p
            WHERE p.AppUserId = u.Id AND p.IsMain = 1
        )
        ORDER BY u.UserName;
    END
    ELSE
    BEGIN
        RAISERROR('You do not have permission to execute this procedure.', 16, 1);
    END
END;
