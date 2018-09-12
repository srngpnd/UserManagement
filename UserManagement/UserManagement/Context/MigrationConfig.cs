using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web;

namespace UserManagement.Context
{
    internal sealed class MigrationConfig : DbMigrationsConfiguration<DataContext>
    {
        public MigrationConfig()
        {
            AutomaticMigrationDataLossAllowed = true;
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(DataContext context)
        {
            Seeding.Go(context);
        }
    }
}