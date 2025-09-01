CREATE  TABLE TolliTest.dbo.ArshatidCostCenter ( 
	Pk                   int    IDENTITY ( 1 , 1 )  NOT NULL,
	Corporation          nvarchar(255)      NOT NULL,
	OrgUnitId            int      NOT NULL,
	OrgUnitName          nvarchar(255)      NOT NULL,
	IsDivision           bit  DEFAULT 0    NOT NULL,
	CostCenterName       nvarchar(255)      NOT NULL,
	CostCenterCode       int      NOT NULL,
	CONSTRAINT pk_ArshatidCostCenter PRIMARY KEY  ( Pk ) 
 );
