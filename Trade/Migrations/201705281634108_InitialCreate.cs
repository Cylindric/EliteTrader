namespace Trade.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.EDStations",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        name = c.String(maxLength: 100),
                        system_id = c.Int(),
                        updated_at = c.Int(),
                        max_landing_pad_size = c.String(),
                        distance_to_star = c.Int(),
                        government_id = c.Int(),
                        government = c.String(),
                        allegiance_id = c.Int(),
                        allegiance = c.String(),
                        state_id = c.Int(),
                        state = c.String(),
                        type_id = c.Int(),
                        type = c.String(),
                        has_blackmarket = c.Boolean(nullable: false),
                        has_market = c.Boolean(nullable: false),
                        has_refuel = c.Boolean(nullable: false),
                        has_repair = c.Boolean(nullable: false),
                        has_rearm = c.Boolean(nullable: false),
                        has_outfitting = c.Boolean(nullable: false),
                        has_shipyard = c.Boolean(nullable: false),
                        has_docking = c.Boolean(nullable: false),
                        has_commodities = c.Boolean(nullable: false),
                        shipyard_updated_at = c.Int(),
                        outfitting_updated_at = c.Int(),
                        market_updated_at = c.Int(),
                        is_planetary = c.Boolean(nullable: false),
                        controlling_minor_faction_id = c.Int(),
                    })
                .PrimaryKey(t => t.id)
                .Index(t => t.name);
            
            CreateTable(
                "dbo.EDSystems",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        edsm_id = c.Int(),
                        name = c.String(maxLength: 100),
                        x = c.Single(),
                        y = c.Single(),
                        z = c.Single(),
                        population = c.Long(),
                        is_populated = c.Int(),
                        government_id = c.Int(),
                        government = c.String(),
                        allegiance_id = c.Int(),
                        allegiance = c.String(),
                        state_id = c.Int(),
                        state = c.String(),
                        security_id = c.Int(),
                        security = c.String(),
                        primary_economy_id = c.Int(),
                        primary_economy = c.String(),
                        power = c.String(),
                        power_state = c.String(),
                        power_state_id = c.Int(),
                        needs_permit = c.Boolean(nullable: false),
                        updated_at = c.Int(),
                        simbad_ref = c.String(),
                        controlling_minor_faction_id = c.Int(),
                        controlling_minor_faction = c.String(),
                        reserve_type_id = c.Int(),
                        reserve_type = c.String(),
                    })
                .PrimaryKey(t => t.id)
                .Index(t => t.name);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.EDSystems", new[] { "name" });
            DropIndex("dbo.EDStations", new[] { "name" });
            DropTable("dbo.EDSystems");
            DropTable("dbo.EDStations");
        }
    }
}
