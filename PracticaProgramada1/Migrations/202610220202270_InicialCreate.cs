namespace PracticaProgramada1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InicialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Categorias",
                c => new
                    {
                        IdCategoria = c.Int(nullable: false, identity: true),
                        NombreCategoria = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.IdCategoria)
                .Index(t => t.NombreCategoria, unique: true, name: "unico_Categoria");
            
            CreateTable(
                "dbo.Productos",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Codigo = c.String(nullable: false, maxLength: 12),
                        Nombre = c.String(nullable: false, maxLength: 80),
                        IdCategoria = c.Int(nullable: false),
                        Precio = c.Decimal(nullable: false, precision: 10, scale: 2),
                        UnidadMedida = c.Int(nullable: false),
                        Existencia = c.Decimal(nullable: false, precision: 10, scale: 3),
                        Activo = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Categorias", t => t.IdCategoria)
                .Index(t => t.Codigo, unique: true, name: "unico_codigo")
                .Index(t => t.IdCategoria);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Productos", "IdCategoria", "dbo.Categorias");
            DropIndex("dbo.Productos", new[] { "IdCategoria" });
            DropIndex("dbo.Productos", "unico_codigo");
            DropIndex("dbo.Categorias", "unico_Categoria");
            DropTable("dbo.Productos");
            DropTable("dbo.Categorias");
        }
    }
}
