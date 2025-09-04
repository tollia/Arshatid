#Arshátið
This solution is to manage registration for the annual celebration event called Árshatid í Icelandic hence the Arshatid Solution name.

It is built on .NET 8.0 and uses C# 12.0 features.
All readable UI Language on in the Razor views are in Icelandic.

## Prerequisites
- .NET 8.0 SDK
- A suitable IDE such as Visual Studio 2022 or Visual Studio Code
- SQL Server or SQL Server Express for database management
- Entity Framework Core tools for database migrations
- Git for version control
- Layout done with Bootstrap 5.3.2
- Execel creation and reading is done by ExcelMapper 6.0.612

## Sub projects
- Arshatid: Admin for configuration and Database mananagement of calls from ArshatidPublic via the IslandIsController all protected by the IslandIs Authorization scheme
  with islandapi routing and each action method routed by a descriptive but simple name.
- ArshatidPublic: Public facing site for event registration and information.
- ArshatidModels: Class library project containing Entity Framework models and other things that need to be shared between Arshatid and ArshatidPublic.

## Database create scripts
Database create scripts are located in the `DatabaseScripts` folder within the solution directory.
Each table is in a separeate file named TableName.sql for easy management.
These all the tables exist in empty form.
This information is to be used to create EF Entity classes in the Models/EF folder in the ArshatidModels project
An empty ArshatidDbContext class can be found in Arshatid project that does inherit from BaseDbContext.
The only object mapped to GeneralDbContext is the Person entity. Add the below to and configure to activate. The table already exists in the database.
    [Table("main_person", Schema = "dbo")]
    public class Person
    {
        [Key]
        [Column("Ssn")]
        [StringLength(10)]
        public string Ssn { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string Name { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Address { get; set; }

        [StringLength(10)]
        [Column("postcode")]
        public string? Postalcode { get; set; }

        [StringLength(255)]
        [Column("province")]
        public string? Municipality { get; set; }
    }
Take care to use all the EntityFramework attributes to ensure the model matches the database table exactly and follows best practices.
Add all Natigation properties as virtual.

# Functionality in Arshatid project
- The authentication is handled by Microsoft Identity with default UI.
- 
## Events
- The main entity is the ArshatidEvent which should in headintgs, titles and main menu be called Viðburðir. Target of the main menu link is the List Events.
### List Events
- The initial page needs to show all Arshatid events already created. 
- A tickbox "Fela liðna atburði" should be available to hide past events.
- In the same line as the tickbox 
- Each event should show the Year, Heading, RegistrationStartTime, RegistrationEndTime, link to edit details. The edit and new event pages should share as much code as possible i.e. an upsert style.
### New Event 
- The upsert page should have all fields available for editing. 
- Creating a new event suffers from the hen and egg problem where a not yet created event does not have a Pk.
  For this reason we allow specifying all values in the Arshatid Entity.
  On saving we want the UI to move onto editing the entry for the freshly created Pk.
### Edit Event
- On saving from edit the UI should also offer a save and a back buttton to go back to the list of events. The filter setting should be restored.
- Offer functions to manage the following related entities from the event edit page.
- - Produce a list of images displaying ArshatidImageType.Name and button to view the full resolution version of the images in a popup, already uploaded for the event with a delete button next to each image. Provide a new button to add more images.
- - Manage the ArshatidInvitees for the event. 
- - - Here we only need a count of invitees a button to add more invitees and a button to upload an entirely new list of Invitees. 
      We first check if any invitees with Registations are excluded and display a count and ask confirmation for deleting them 
      when the Invitation is removed. It should handle uploading of text, csv and excel files. In the case of excel we assume only one workbook and pick the first column
      named KT or Kennitala in a case insensitive manner, the FullName should be taken from a column called Nafn.
- - - Offer a button to edit the Invitee list on a new page. Use the Person to obtain the Name for the entered Ssn.
## Registrations
- The Registrations page should show a histogram of registration per day where the bars are horizontal following the date it relates to. Order by ascending date. 
- - Above the histogram a button that cretes and downloads registrations as they stand now. To the left of that button is a count of participants by invitee and how many plus the have registered in total.
## The IslandIsController behind the islandapi route
- The IslandIsController is the only controller in the Arshatid project that is protected by the IslandIs authorization scheme.
- Please note that the Ssn for the call context comes from the nationalId claim in the token.
- In generaal the context Ssn from the token scopes the objects that can be acted upon. This needs to be does through eamining the ArshatidInvitee tabele as that is the only place where we have the Ssn in the datamodel.
- GetRegistration 
- - This returns the ArsahatidRegistration for the Ssn in the call context and the current event if any.
- - If no current event exists or the Ssn is not registered null is returned.
- - If the ssn is not an invitee for the current event a 400 Bad Request is returned. 
- - - The ArsahatidRegistration needs to have the ArshatidInvitee populated.
- UpsertRegistration
- - allow registration to be changed. In this case there is only the Plus field that should at the moment be presented by a tickbox where not ticked means 0 and ticked means 1.
- - - If no current event exists or the Ssn is not an invitee for the Ssn in the call context a 400 Bad Request is returned.
- DeleteRegistration