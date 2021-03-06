-- DROP TABLE dbo.SmartUser
CREATE TABLE dbo.SmartUser
(
	Idx INT IDENTITY(1,1) 
	,[UserName] NVARCHAR(50) NOT NULL
	,[PassWord] NVARCHAR(50) NOT NULL
	,[Salt] NVARCHAR(32) NOT NULL
	,[Email] NVARCHAR(200)
	,[Phone] NVARCHAR(25)
	,[Avatar] NVARCHAR(50)
	,[State] INT NOT NULL
	,[CreateOn] DATETIME NOT NULL
	,[CreateBy] NVARCHAR(50) NOT NULL
	,[LastModifyOn] DATETIME NOT NULL
	,[LastModifyBy] NVARCHAR(50) NOT NULL
	,CONSTRAINT PK_SmartUser_Name PRIMARY KEY NONCLUSTERED(UserName)	--创建主键且主键索引为非聚集
	,CONSTRAINT IX_SmartUser_Idx UNIQUE CLUSTERED(Idx)	--聚集索引
)
execute sp_addextendedproperty N'MS_Description', N'用户名',N'user', N'dbo', N'table', N'SmartUser', N'column', N'UserName'
execute sp_addextendedproperty N'MS_Description', N'密码',N'user', N'dbo', N'table', N'SmartUser', N'column', N'PassWord'
execute sp_addextendedproperty N'MS_Description', N'密钥盐',N'user', N'dbo', N'table', N'SmartUser', N'column', N'Salt'
execute sp_addextendedproperty N'MS_Description', N'头像',N'user', N'dbo', N'table', N'SmartUser', N'column', N'Avatar'
go
INSERT INTO dbo.SmartUser
SELECT N'Admin',N'U/aAGsy/Qo1F9FAYDF6oTQ==',N'd93c258408ae4fcc88e9c87b92f8d531',N'767206664@qq.com',N'17328532458',N'http://localhost:5000/Image/1.png',2,GETDATE(),SYSTEM_USER,GETDATE(),SYSTEM_USER

GO
CREATE TABLE dbo.SmartRole
(
	Idx INT IDENTITY(1,1) 
	,[RoleName] NVARCHAR(50) NOT NULL
	,[ParentIdx] INT
	,[RoleCode] NVARCHAR(50) NOT NULL
	,[pgRole] BIT DEFAULT 0 NOT NULL
	,[Description] NVARCHAR(200)
	,[State] INT NOT NULL
	,[CreateOn] DATETIME NOT NULL
	,[CreateBy] NVARCHAR(50) NOT NULL
	,[LastModifyOn] DATETIME NOT NULL
	,[LastModifyBy] NVARCHAR(50) NOT NULL
	,CONSTRAINT PK_SmartRole_Name PRIMARY KEY NONCLUSTERED(RoleName)	--创建主键且主键索引为非聚集
	,CONSTRAINT FK_SmartRole_ParentIdx FOREIGN KEY(ParentIdx) REFERENCES dbo.SmartRole(Idx)
	,CONSTRAINT IX_SmartRole_RoleName_Idx UNIQUE CLUSTERED(Idx)	--聚集索引
)
execute sp_addextendedproperty N'MS_Description', N'角色',N'user', N'dbo', N'table', N'SmartRole', N'column', N'RoleName'
execute sp_addextendedproperty N'MS_Description', N'上级角色Id',N'user', N'dbo', N'table', N'SmartRole', N'column', N'ParentIdx'
execute sp_addextendedproperty N'MS_Description', N'角色类型代码',N'user', N'dbo', N'table', N'SmartRole', N'column', N'RoleCode'

INSERT INTO dbo.SmartRole([RoleName],[ParentIdx],[RoleCode],[Description],[State],[CreateOn],[CreateBy],[LastModifyOn],[LastModifyBy])
SELECT N'系统开发员角色',NULL,N'Developer',NULL,2,GETDATE(),SYSTEM_USER,GETDATE(),SYSTEM_USER
UNION ALL
SELECT N'系统管理员角色',NULL,N'Admin',NULL,2,GETDATE(),SYSTEM_USER,GETDATE(),SYSTEM_USER
UNION ALL
SELECT N'用户操作角色',NULL,N'Operate',NULL,2,GETDATE(),SYSTEM_USER,GETDATE(),SYSTEM_USER
UNION ALL
SELECT N'数据查询角色',NULL,N'View',NULL,2,GETDATE(),SYSTEM_USER,GETDATE(),SYSTEM_USER
UNION ALL
SELECT N'审批角色',NULL,N'Approval',NULL,2,GETDATE(),SYSTEM_USER,GETDATE(),SYSTEM_USER

CREATE TABLE dbo.SmartUserInRole
(
	Idx INT IDENTITY(1,1) 
	,[RoleIdx] INT NOT NULL
	,[UserIdx] INT NOT NULL
	,[CreateOn] DATETIME NOT NULL
	,[CreateBy] NVARCHAR(50) NOT NULL
	,[LastModifyOn] DATETIME NOT NULL
	,[LastModifyBy] NVARCHAR(50) NOT NULL
	,CONSTRAINT PK_SmartUserInRole_Idx PRIMARY KEY CLUSTERED(Idx)
	,INDEX IX_SmartUserInRole_UserIdx NONCLUSTERED(UserIdx)
	,INDEX IX_SmartUserInRole_RoleIdx NONCLUSTERED(RoleIdx)
)
INSERT INTO dbo.SmartUserInRole
SELECT 1,1,GETDATE(),SYSTEM_USER,GETDATE(),SYSTEM_USER

CREATE TABLE dbo.SmartUserInRoleLog
(
	Idx INT IDENTITY(1,1) 
	,[RoleIdx] INT NOT NULL
	,[UserIdx] INT NOT NULL
	,[CreateOn] DATETIME NOT NULL
	,[CreateBy] NVARCHAR(50) NOT NULL
	,CONSTRAINT PK_SmartUserInRoleLog_Idx PRIMARY KEY CLUSTERED(Idx)
)

CREATE TABLE dbo.SmartPermission
(
	Idx INT IDENTITY(1,1) 
	,[PermissionName] NVARCHAR(50) NOT NULL
	,[ParentIdx] INT
	,[Level] NCHAR(2) NOT NULL CHECK([Level]=N'SA' OR [Level]=N'MA' OR [Level]=N'MP')
	,[Description] NVARCHAR(100)
	,[State] INT NOT NULL
	,[CreateOn] DATETIME NOT NULL
	,[CreateBy] NVARCHAR(50) NOT NULL
	,[LastModifyOn] DATETIME NOT NULL
	,[LastModifyBy] NVARCHAR(50) NOT NULL
	,CONSTRAINT PK_SmartPermission_Name PRIMARY KEY NONCLUSTERED(PermissionName)	--创建主键且主键索引为非聚集
	,CONSTRAINT IX_SmartPermission_Idx UNIQUE CLUSTERED(Idx)	--聚集索引
	,CONSTRAINT FK_SmartPermission_ParentIdx FOREIGN KEY(ParentIdx) REFERENCES dbo.SmartPermission(Idx)
)

INSERT INTO dbo.SmartPermission
SELECT N'系统管理权限类',NULL,N'SA',NULL,2,GETDATE(),SYSTEM_USER,GETDATE(),SYSTEM_USER
UNION ALL
SELECT N'我的任务权限类',NULL,N'SA',NULL,2,GETDATE(),SYSTEM_USER,GETDATE(),SYSTEM_USER
UNION ALL
SELECT N'我的管理权限类',NULL,N'SA',NULL,2,GETDATE(),SYSTEM_USER,GETDATE(),SYSTEM_USER
UNION ALL
SELECT N'工作积累示例权限类',NULL,N'SA',NULL,2,GETDATE(),SYSTEM_USER,GETDATE(),SYSTEM_USER
UNION ALL
SELECT N'用户管理',1,N'MP',NULL,2,GETDATE(),SYSTEM_USER,GETDATE(),SYSTEM_USER
UNION ALL
SELECT N'角色管理',1,N'MP',NULL,2,GETDATE(),SYSTEM_USER,GETDATE(),SYSTEM_USER
UNION ALL
SELECT N'用户角色关系管理',1,N'MP',NULL,2,GETDATE(),SYSTEM_USER,GETDATE(),SYSTEM_USER
UNION ALL
SELECT N'权限管理',1,N'MP',NULL,2,GETDATE(),SYSTEM_USER,GETDATE(),SYSTEM_USER
UNION ALL
SELECT N'角色权限关系管理',1,N'MP',NULL,2,GETDATE(),SYSTEM_USER,GETDATE(),SYSTEM_USER
UNION ALL
SELECT N'业务模块管理',1,N'MP',NULL,2,GETDATE(),SYSTEM_USER,GETDATE(),SYSTEM_USER
UNION ALL
SELECT N'页面管理',1,N'MP',NULL,2,GETDATE(),SYSTEM_USER,GETDATE(),SYSTEM_USER

CREATE TABLE dbo.SmartRoleInPermission
(
	Idx INT IDENTITY(1,1) 
	,[RoleIdx] INT NOT NULL
	,[PermissionIdx] INT NOT NULL
	,[opIdx] INT NOT NULL DEFAULT 0
	,[CreateOn] DATETIME NOT NULL
	,[CreateBy] NVARCHAR(50) NOT NULL
	,CONSTRAINT PK_SmartRoleInPermission_Idx PRIMARY KEY CLUSTERED(Idx)
	,INDEX IX_SmartRoleInPermission_PermissionIdx NONCLUSTERED(PermissionIdx)
	,INDEX IX_SmartRoleInPermission_RoleIdx NONCLUSTERED(RoleIdx)
)
INSERT INTO SmartRoleInPermission
SELECT 1,1,31,GETDATE(),SYSTEM_USER
UNION ALL
SELECT 1,2,31,GETDATE(),SYSTEM_USER
UNION ALL
SELECT 1,3,31,GETDATE(),SYSTEM_USER
UNION ALL
SELECT 1,4,31,GETDATE(),SYSTEM_USER
UNION ALL
SELECT 1,5,31,GETDATE(),SYSTEM_USER
UNION ALL
SELECT 1,6,31,GETDATE(),SYSTEM_USER
UNION ALL
SELECT 1,7,31,GETDATE(),SYSTEM_USER
UNION ALL
SELECT 1,8,31,GETDATE(),SYSTEM_USER
UNION ALL
SELECT 1,9,31,GETDATE(),SYSTEM_USER
UNION ALL
SELECT 1,10,31,GETDATE(),SYSTEM_USER
UNION ALL
SELECT 1,11,31,GETDATE(),SYSTEM_USER

CREATE TABLE dbo.SmartRoleInPermissionLog
(
	Idx INT IDENTITY(1,1) 
	,[RoleIdx] INT NOT NULL FOREIGN KEY REFERENCES dbo.SmartRole(Idx)
	,[PermissionIdx] INT NOT NULL
	,[opIdx] INT NOT NULL
	,[CreateOn] DATETIME NOT NULL
	,[CreateBy] NVARCHAR(50) NOT NULL
	,CONSTRAINT PK_SmartRoleInPermissionLog_Idx PRIMARY KEY CLUSTERED(Idx)
	,CONSTRAINT[FK_SmartRoleInPermissionLog_PermissionIdx] FOREIGN KEY(PermissionIdx) REFERENCES dbo.SmartPermission(Idx)
)
GO
CREATE TABLE dbo.SmartPages
(
	Idx INT IDENTITY(1,1) 
	,[PageAction] NVARCHAR(50) NOT NULL
	,[Link] NVARCHAR(255)
	,[Description] NVARCHAR(100)
	,[SortOrder] INT DEFAULT 0 NOT NULL
	,[State] INT NOT NULL
	,[CreateOn] DATETIME NOT NULL
	,[CreateBy] NVARCHAR(50) NOT NULL
	,[LastModifyOn] DATETIME NOT NULL
	,[LastModifyBy] NVARCHAR(50) NOT NULL
	,CONSTRAINT PK_SmartPages_PageRole PRIMARY KEY NONCLUSTERED(PageAction)	--创建主键且主键索引为非聚集
	,CONSTRAINT IX_SmartPages_Idx UNIQUE CLUSTERED(Idx)	--聚集索引
)
INSERT INTO dbo.SmartPages
SELECT N'SmartUser',N'/SystemManage/SmartUser',NULL,1,2,GETDATE(),SYSTEM_USER,GETDATE(),SYSTEM_USER
UNION ALL
SELECT N'SmartRole',N'/SystemManage/SmartRole',NULL,2,2,GETDATE(),SYSTEM_USER,GETDATE(),SYSTEM_USER
UNION ALL
SELECT N'SmartUserInRole',N'/SystemManage/SmartUserInRole',NULL,3,2,GETDATE(),SYSTEM_USER,GETDATE(),SYSTEM_USER
UNION ALL
SELECT N'SmartPermission',N'/SystemManage/SmartPermission',NULL,4,2,GETDATE(),SYSTEM_USER,GETDATE(),SYSTEM_USER
UNION ALL
SELECT N'SmartRoleInPermission',N'/SystemManage/SmartRoleInPermission',NULL,5,2,GETDATE(),SYSTEM_USER,GETDATE(),SYSTEM_USER
UNION ALL
SELECT N'SmartModule',N'/SystemManage/SmartModule',NULL,6,2,GETDATE(),SYSTEM_USER,GETDATE(),SYSTEM_USER
UNION ALL
SELECT N'SmartPages',N'/SystemManage/SmartPages',NULL,7,2,GETDATE(),SYSTEM_USER,GETDATE(),SYSTEM_USER


GO
CREATE TABLE dbo.SmartModule
(
	Idx INT IDENTITY(1,1) 
	,[ModuleName] NVARCHAR(50) NOT NULL
	,[ParentIdx] INT 
	,[PermissionIdx] INT NOT NULL
	,[PageIdx] INT NULL
	,[Description] NVARCHAR(100)
	,[ICON] NVARCHAR(50)
	,[SortOrder] INT DEFAULT 0 NOT NULL
	,[State] INT NOT NULL
	,[CreateOn] DATETIME NOT NULL
	,[CreateBy] NVARCHAR(50) NOT NULL
	,[LastModifyOn] DATETIME NOT NULL
	,[LastModifyBy] NVARCHAR(50) NOT NULL
	,CONSTRAINT PK_SmartModule_Name PRIMARY KEY NONCLUSTERED(ModuleName)	--创建主键且主键索引为非聚集
	,CONSTRAINT FK_SmartModule_ParentIdx FOREIGN KEY(ParentIdx) REFERENCES dbo.SmartModule(Idx)
	,CONSTRAINT FK_SmartModule_PermissionIdx FOREIGN KEY(PermissionIdx) REFERENCES dbo.SmartPermission(Idx)
	,CONSTRAINT FK_SmartModule_PageIdx FOREIGN KEY(PageIdx) REFERENCES dbo.SmartPages(Idx)
	,CONSTRAINT IX_SmartModule_Idx UNIQUE CLUSTERED(Idx)	--聚集索引
	,INDEX IX_SmartModule_ParentIdx NONCLUSTERED(ParentIdx)-- 非聚集索引
)
INSERT INTO dbo.SmartModule
SELECT N'系统管理',NULL,1,NULL,NULL,NULL,9,2,GETDATE(),SYSTEM_USER,GETDATE(),SYSTEM_USER
UNION ALL
SELECT N'我的任务',NULL,2,NULL,NULL,NULL,1,2,GETDATE(),SYSTEM_USER,GETDATE(),SYSTEM_USER
UNION ALL
SELECT N'我的管理',NULL,3,NULL,NULL,NULL,2,2,GETDATE(),SYSTEM_USER,GETDATE(),SYSTEM_USER
UNION ALL
SELECT N'工作积累示例',NULL,4,NULL,NULL,NULL,3,2,GETDATE(),SYSTEM_USER,GETDATE(),SYSTEM_USER
UNION ALL
SELECT N'用户管理',1,5,1,NULL,NULL,1,2,GETDATE(),SYSTEM_USER,GETDATE(),SYSTEM_USER
UNION ALL
SELECT N'角色管理',1,6,2,NULL,NULL,2,2,GETDATE(),SYSTEM_USER,GETDATE(),SYSTEM_USER
UNION ALL
SELECT N'用户角色关系管理',1,7,3,NULL,NULL,3,2,GETDATE(),SYSTEM_USER,GETDATE(),SYSTEM_USER
UNION ALL
SELECT N'权限管理',1,8,4,NULL,NULL,4,2,GETDATE(),SYSTEM_USER,GETDATE(),SYSTEM_USER
UNION ALL
SELECT N'角色权限关系管理',1,9,5,NULL,NULL,5,2,GETDATE(),SYSTEM_USER,GETDATE(),SYSTEM_USER
UNION ALL
SELECT N'业务模块管理',1,10,6,NULL,NULL,6,2,GETDATE(),SYSTEM_USER,GETDATE(),SYSTEM_USER
UNION ALL
SELECT N'页面管理',1,11,7,NULL,NULL,6,2,GETDATE(),SYSTEM_USER,GETDATE(),SYSTEM_USER

GO
CREATE TABLE dbo.SmartOperate
(
	Idx SMALLINT NOT NULL 
	,[opName] NVARCHAR(50) NOT NULL
	,[opCode] NVARCHAR(50) NOT NULL
	,[opDescription] NVARCHAR(200)
	,[ICon] NVARCHAR(50)
	,[sortOrder] INT NOT NULL DEFAULT 0
	,[State] INT NOT NULL
	,[CreateOn] DATETIME NOT NULL
	,[CreateBy] NVARCHAR(50) NOT NULL
	,[LastModifyOn] DATETIME NOT NULL
	,[LastModifyBy] NVARCHAR(50) NOT NULL
	,CONSTRAINT PK_SmartOperate_Name PRIMARY KEY NONCLUSTERED(opName)	--创建主键且主键索引为非聚集
	,CONSTRAINT IX_SmartOperate_Idx UNIQUE CLUSTERED(Idx)	--聚集索引
)
INSERT INTO dbo.SmartOperate(opName,opCode,opDescription,ICon,Idx,[CreateOn] ,[CreateBy],[LastModifyOn],[LastModifyBy],[State])
SELECT N'添',N'Add',N'添加',NULL,1,GETDATE(),SYSTEM_USER,GETDATE(),SYSTEM_USER,2
UNION ALL
SELECT N'删',N'Del',N'删除',NULL,2,GETDATE(),SYSTEM_USER,GETDATE(),SYSTEM_USER,2
UNION ALL
SELECT N'改',N'Update',N'修改',NULL,4,GETDATE(),SYSTEM_USER,GETDATE(),SYSTEM_USER,2
UNION ALL
SELECT N'查',N'View',N'查看',NULL,8,GETDATE(),SYSTEM_USER,GETDATE(),SYSTEM_USER,2
UNION ALL
SELECT N'刷新',N'Refresh',N'刷新',NULL,16,GETDATE(),SYSTEM_USER,GETDATE(),SYSTEM_USER,2
UNION ALL
SELECT N'上传',N'Del',N'上传',NULL,32,GETDATE(),SYSTEM_USER,GETDATE(),SYSTEM_USER,2
UNION ALL
SELECT N'下载',N'Del',N'下载',NULL,64,GETDATE(),SYSTEM_USER,GETDATE(),SYSTEM_USER,2
UNION ALL
SELECT N'导出Excel',N'Del',N'导出Excel',NULL,128,GETDATE(),SYSTEM_USER,GETDATE(),SYSTEM_USER,2
UNION ALL
SELECT N'导入Excel',N'Del',N'导入Excel',NULL,256,GETDATE(),SYSTEM_USER,GETDATE(),SYSTEM_USER,2
UNION ALL
SELECT N'导出PDF',N'Del',N'导出PDF',NULL,512,GETDATE(),SYSTEM_USER,GETDATE(),SYSTEM_USER,2

GO
CREATE TABLE dbo.SmartUserModule
(
	Idx INT IDENTITY(1,1) NOT NULL
	,[UserIdx] INT NOT NULL
	,[ModuleIdx] INT NOT NULL
	,[PageIdx] INT NOT NULL
	,[opIdx] INT NOT NULL
	,CONSTRAINT PK_SmartUserModule_UserIdx_ModuleIdx PRIMARY KEY(UserIdx,ModuleIdx)
	,CONSTRAINT FK_SmartUserModule_UserIdx FOREIGN KEY(UserIdx) REFERENCES dbo.SmartUser(Idx)
	,CONSTRAINT FK_SmartUserModule_ModuleIdx FOREIGN KEY(ModuleIdx) REFERENCES dbo.SmartModule(Idx)
	,CONSTRAINT FK_SmartUserModule_PageIdx FOREIGN KEY(PageIdx) REFERENCES dbo.SmartPages(Idx)
	,CONSTRAINT IX_SmartUserModule_Idx UNIQUE CLUSTERED(Idx)	--聚集索引
)