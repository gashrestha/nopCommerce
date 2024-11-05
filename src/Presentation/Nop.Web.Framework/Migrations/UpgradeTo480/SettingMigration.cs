﻿using FluentMigrator;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Tax;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Configuration;

namespace Nop.Web.Framework.Migrations.UpgradeTo480;

[NopUpdateMigration("2024-11-01 00:00:00", "4.80", UpdateMigrationType.Settings)]
public class SettingMigration : MigrationBase
{
    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //do not use DI, because it produces exception on the installation process
        var settingService = EngineContext.Current.Resolve<ISettingService>();

        //#7215
        var displayAttributeCombinationImagesOnly = settingService.GetSetting("producteditorsettings.displayattributecombinationimagesonly");
        if (displayAttributeCombinationImagesOnly is not null)
            settingService.DeleteSetting(displayAttributeCombinationImagesOnly);

        //#7325
        var orderSettings = settingService.LoadSetting<OrderSettings>();
        if (!settingService.SettingExists(orderSettings, settings => settings.PlaceOrderWithLock))
        {
            orderSettings.PlaceOrderWithLock = false;
            settingService.SaveSetting(orderSettings, settings => settings.PlaceOrderWithLock);
        }
        
        //#7394
        if (orderSettings.MinimumOrderPlacementInterval > 10)
        {
            if (orderSettings.MinimumOrderPlacementInterval < 60)
                orderSettings.MinimumOrderPlacementInterval = 1;
            else
                orderSettings.MinimumOrderPlacementInterval = Math.Truncate(orderSettings.MinimumOrderPlacementInterval / 60.0) + (orderSettings.MinimumOrderPlacementInterval % 60) == 0 ? 0 : 1;

            settingService.SaveSetting(orderSettings, settings => settings.MinimumOrderPlacementInterval);
        }

        //#7265
        var taxSetting = settingService.LoadSetting<TaxSettings>();
        if (!settingService.SettingExists(taxSetting, settings => settings.EuVatRequired))
        {
            taxSetting.EuVatRequired = false;
            settingService.SaveSetting(taxSetting, settings => settings.EuVatRequired);
        }

        //#4306
        var catalogSettings = settingService.LoadSetting<CatalogSettings>();
        if (!settingService.SettingExists(catalogSettings, settings => settings.ShowSearchBoxCategories))
        {
            catalogSettings.ShowSearchBoxCategories = false;
            settingService.SaveSetting(catalogSettings, settings => settings.ShowSearchBoxCategories);
        }
    }

    public override void Down()
    {
        //add the downgrade logic if necessary 
    }
}