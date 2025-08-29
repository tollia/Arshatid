CREATE  TABLE TolliTest.dbo.ArshatidInvitee ( 
	pk                   int    IDENTITY ( 1 , 1 )  NOT NULL,
	Ssn                  varchar(10)      NOT NULL,
	FullName             nvarchar(255)      NOT NULL,
	ArshatidFk           int      NOT NULL,
	CONSTRAINT pk_ArshatidInvitees PRIMARY KEY  ( pk ) ,
	CONSTRAINT ArshatidSsnLen CHECK ( len([Ssn])=(10) )
 );

ALTER TABLE TolliTest.dbo.ArshatidInvitee ADD CONSTRAINT fk_arshatidinvitees_arshatid FOREIGN KEY ( ArshatidFk ) REFERENCES TolliTest.dbo.Arshatid( Pk );


