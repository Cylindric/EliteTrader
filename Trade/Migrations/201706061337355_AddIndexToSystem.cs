namespace Trade.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class AddIndexToSystem : DbMigration
    {
        public override void Up()
        {
            AddColumn("EDSystem", "location", c => c.Geometry());
            Sql("CREATE SPATIAL INDEX ixGeomTest ON EDsystems(location) WITH(BOUNDING_BOX = (xmin = -44000, ymin = -18000, xmax = 41000, ymax = 5500))");
        }

        public override void Down()
        {
            Sql("DROP INDEX IF EXISTS ixGeomTest ON EDsystem");
            DropColumn("EDSystem", "location");
        }
    }
}
