using FluentMigrator;

namespace Migrator.Migrations;

[TimestampedMigration(2023, 9, 17, 16 ,8)]
public sealed class AddFileTable : ForwardOnlyMigration 
{
    private const string CreateExtension = @"CREATE EXTENSION IF NOT EXISTS ""uuid-ossp"";";
    
    public override void Up()
    {
        Execute.Sql(CreateExtension);
        
        Create.Table("File")
            .WithColumn("Id").AsGuid().WithDefault(SystemMethods.NewGuid).PrimaryKey()
            .WithColumn("VirtualName").AsString(75).NotNullable()
            .WithColumn("Path").AsString(300).NotNullable()
            .WithColumn("AccessToken").AsString(50).Nullable().Indexed()
            .WithColumn("ToDelete").AsBoolean().NotNullable().Indexed();
    }
}