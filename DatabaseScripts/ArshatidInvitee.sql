CREATE  TABLE TolliTest.dbo.ArshatidInvitee ( 
	pk                   int    IDENTITY ( 1 , 1 )  NOT NULL,
	Ssn                  varchar(10)      NOT NULL,
	FullName             nvarchar(255)      NOT NULL,
	ArshatidFk           int      NOT NULL,
	CONSTRAINT pk_ArshatidInvitees PRIMARY KEY  ( pk ) ,
	CONSTRAINT unq_ArshatidInvitee UNIQUE ( ArshatidFk, Ssn ) ,
	CONSTRAINT ck_ArshatidInvitee_SsnLen CHECK ( len([Ssn])=(10) ),
	CONSTRAINT ck_ArshatidInvitee_SsnDigits CHECK ( NOT [Ssn] like '%[^0-9]%' )
 );

ALTER TABLE TolliTest.dbo.ArshatidInvitee ADD CONSTRAINT fk_arshatidinvitees_arshatid FOREIGN KEY ( ArshatidFk ) REFERENCES TolliTest.dbo.Arshatid( Pk );


