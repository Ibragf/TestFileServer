using FluentMigrator;

namespace Migrator.Migrations;

[TimestampedMigration(2023, 9, 17, 20, 31)]
public sealed class AddAccessTokenTable : ForwardOnlyMigration
{
    public override void Up()
    {
        Create.Table("AccessToken")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("Token").AsString(50).NotNullable().Indexed()
            .WithColumn("CloudFileId").AsGuid().NotNullable().ForeignKey("File", "Id");
    }
}