CREATE  TABLE TolliTest.dbo.ArshatidImage ( 
	Pk                   int    IDENTITY ( 1 , 1 )  NOT NULL,
	ArshatifFk           int      NOT NULL,
	ContentType          varchar(255)      NOT NULL,
	ImageData            varbinary(max)      NOT NULL,
	ImageTypeFk          int      NOT NULL,
	CONSTRAINT pk_ArshatidImage PRIMARY KEY  ( Pk ) 
 );

ALTER TABLE TolliTest.dbo.ArshatidImage ADD CONSTRAINT fk_arshatidimage_arshatid FOREIGN KEY ( ArshatifFk ) REFERENCES TolliTest.dbo.Arshatid( Pk );

ALTER TABLE TolliTest.dbo.ArshatidImage ADD CONSTRAINT fk_arshatidimage FOREIGN KEY ( ImageTypeFk ) REFERENCES TolliTest.dbo.ArshatidImageType( Pk );

