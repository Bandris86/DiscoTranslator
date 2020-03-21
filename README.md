# DiscoTranslator

Disco Elysium translation plugin for [BepInEx](https://github.com/BepInEx/BepInEx) v5.0+ (not RC1) framework.

## How this thing works

Disco Elysium already has [I2 Localization](https://assetstore.unity.com/packages/tools/localization/i2-localization-14884) translation framework in it.

This plugin hooks translation request(`I2.Loc.LocalizationManager.GetTranslation()`) and return your translated text instead.

## Usage

Press <kbd>Alt</kbd> + <kbd>T</kbd> (configurable) to open GUI window. Press again to close.

## Features

### 1. Translation loader

Load `gettext PO` files from `DiscoTranslator\Translation` directory.

### 2. Runtime translation reload

Press <kbd>Alt</kbd> + <kbd>R</kbd> (configurable) or press `Reload translation` button from UI to reload all translation files.

Reloaded translations will be applied when new texts appear or UI component reappears.

### 3. Catalog export

Press `Export catalog` button from UI. The plugin will create `gettext POT` catalog files at `DiscoTranslator\Catalog` directory.

### 4. Database export

Press `Export dialogue database` button from UI. The plugin will create `database.json` file at `DiscoTranslator` directory.
