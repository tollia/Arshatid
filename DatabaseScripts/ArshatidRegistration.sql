CREATE  TABLE TolliTest.dbo.ArshatidRegistration ( 
	Pk                   int    IDENTITY ( 1 , 1 )  NOT NULL,
	Plus                 int  DEFAULT 0    NOT NULL,
	Alergies             nvarchar(255)      NULL,
	ArshatidInviteeFk    int      NOT NULL,
	Vegan                bit  DEFAULT 0    NOT NULL,
	ArshatidCostCenterFk int      NOT NULL,
	CONSTRAINT pk_ArshatidRegistrations PRIMARY KEY  ( Pk ) ,
	CONSTRAINT unq_ArshatidRegistration_Invitee UNIQUE ( ArshatidInviteeFk ) 
 );

ALTER TABLE TolliTest.dbo.ArshatidRegistration ADD CONSTRAINT fk_arshatidregistration FOREIGN KEY ( ArshatidInviteeFk ) REFERENCES TolliTest.dbo.ArshatidInvitee( pk );

ALTER TABLE TolliTest.dbo.ArshatidRegistration ADD CONSTRAINT fk_arshatidregistration_costcenter FOREIGN KEY ( ArshatidCostCenterFk ) REFERENCES TolliTest.dbo.ArshatidCostCenter( Pk );
