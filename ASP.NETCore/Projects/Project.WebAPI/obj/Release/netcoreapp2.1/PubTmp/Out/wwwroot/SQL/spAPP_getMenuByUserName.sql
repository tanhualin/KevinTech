/*
	获取菜单列表
*/
CREATE PROC [dbo].[spAPP_getMenuByUserName]
	@UserName NVARCHAR(50)
AS
SET NOCOUNT ON;

SELECT 
	m.Idx
	,m.ModuleName
	,m.ParentIdx
	,ISNULL(oPage.Link,p.Link) AS Link
	,ISNULL(oPage.opIdx,rp.opIdx) AS opIdx
	,m.ICON
	,m.SortOrder
FROM dbo.SmartModule m
JOIN dbo.SmartRoleInPermission rp ON rp.PermissionIdx=m.PermissionIdx
JOIN dbo.SmartUserInRole ur ON ur.RoleIdx=rp.RoleIdx
JOIN dbo.SmartUser u ON u.Idx=ur.UserIdx
LEFT JOIN dbo.SmartPages p ON p.Idx=m.PageIdx
OUTER APPLY
(
	SELECT 
		um.opIdx
		,pg.Link 
	FROM dbo.SmartUserModule um 
	JOIN dbo.SmartPages pg ON pg.Idx = um.Idx
	WHERE um.UserIdx=u.Idx
		AND um.ModuleIdx=m.Idx
)oPage
WHERE u.UserName=@UserName
	AND m.[State]=2
ORDER BY m.SortOrder 