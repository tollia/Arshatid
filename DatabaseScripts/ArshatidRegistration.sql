CREATE  TABLE TolliTest.dbo.ArshatidRegistration ( 
	Pk                   int    IDENTITY ( 1 , 1 )  NOT NULL,
	Ssn                  varchar(10)      NOT NULL,
	Plus                 int  DEFAULT 0    NOT NULL,
	ArshatidFk           int      NOT NULL,
	Alergies             nvarchar(255)      NULL,
	ArshatidInviteeFk    int      NOT NULL,
	CONSTRAINT pk_ArshatidRegistrations PRIMARY KEY  ( Pk ) ,
	CONSTRAINT unq_ArshatidRegistrations UNIQUE ( ArshatidFk, Ssn ) 
 );

ALTER TABLE TolliTest.dbo.ArshatidRegistration ADD CONSTRAINT fk_arshatidregistrations FOREIGN KEY ( ArshatidFk ) REFERENCES TolliTest.dbo.Arshatid( Pk );

ALTER TABLE TolliTest.dbo.ArshatidRegistration ADD CONSTRAINT fk_arshatidregistration FOREIGN KEY ( ArshatidInviteeFk ) REFERENCES TolliTest.dbo.ArshatidInvitee( pk );

