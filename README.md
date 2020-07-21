# DiscoTranslator

Disco Elysium translation plugin for [BepInEx](https://github.com/BepInEx/BepInEx) v5.0+ (not RC1) framework.

## How this thing works

Disco Elysium already has [I2 Localization](https://assetstore.unity.com/packages/tools/localization/i2-localization-14884) translation framework in it.

This plugin hooks translation request(`I2.Loc.LocalizationManager.GetTranslation()`) and return your translated text instead.

## Usage

Press <kbd>Alt</kbd> + <kbd>T</kbd> (configurable) to open GUI window. Press again to close.

## Features

### Translation loader

Load `gettext PO` files from `DiscoTranslator\Translation` directory.
Load `gettext PO` files from `DiscoTranslator\Translation` directory.


### Runtime translation reload

Press <kbd>Alt</kbd> + <kbd>R</kbd> or <kbd>Reload translation</kbd> button from UI to reload all translation files.

Reloaded translations will be applied when new texts appear or UI component reappears.

### Catalog export

Press <kbd>Export catalog</kbd> button from UI. The plugin will create `gettext POT` catalog files at `DiscoTranslator\Catalog` directory.

### Image translation

Press <kbd>Export images</kbd> button from UI to extract images.

Translate images at `DiscoTranslator\OriginalImages` and put them into `DiscoTranslator\Images` folder.

### Database export

Press `Export dialogue database` button from UI. The plugin will create `database.json` file at `DiscoTranslator` directory. És a végére is írok valamit.

