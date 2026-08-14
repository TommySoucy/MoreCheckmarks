# Adding a language to MoreCheckmarks

Every string MoreCheckmarks displays itself lives in this folder, one JSON file per language.
Adding a language means adding a file here - no rebuild, no code change.

Text that belongs to the game (item names, quest names, trader names, hideout area names, and
labels like `STASH` or `Barter`) is *not* in these files. The mod asks the game for those, so they
are already translated wherever the game itself is translated.

## 1. Name the file after the game's language code

Copy `en.json` to `<code>.json`. **The codes are the game's own, and they are not always the usual
two-letter ones.** The one that catches people out is German:

| Language | File       |
|----------|------------|
| English  | `en.json`  |
| French   | `fr.json`  |
| Russian  | `ru.json`  |
| German   | `ge.json`  |

`ge`, not `de`. If you are unsure of a code, the game's own locale files are named the same way.

The mod picks the file matching the language the game is running in. Players can also force one
from the F12 configuration menu, which is worth doing while you work on a translation. Either way
the game has to be restarted for a change to take effect.

## 2. Translate the values, never the keys

Keys on the left stay exactly as they are. Only the text on the right gets translated.

```json
"tooltip.wishlist.line": "\nOn <color=#{color}>Wish List</color>"
```

Anything you leave out falls back to English, so a half-finished file is safe to ship and safe to
test with. There is no way for a missing entry to produce an empty tooltip.

## 3. Keep the `{placeholders}`

`{count}`, `{trader}`, `{level}` and friends are replaced with real values at runtime. Keep every
one that appears in the English string - but move them wherever your language needs them:

```json
"tooltip.craft.recipe": "\n    <color=#{color}>{item} lvl{level}</color> ({amount})"
```

A placeholder you drop simply never appears; one you misspell shows up as literal `{typo}` text.
Write a literal brace as `{{`.

## 4. Keep the markup

`<color=#...>`, `</color>` and `\n` are rich text and line breaks, not words. Leave them in place
and translate around them. Where a colour is configurable it arrives as `{color}` and must stay.

## 5. Plurals: `.one` and `.other`

Keys ending in `.one` and `.other` are two wordings of the same message. Which one is used depends
on the rule declared at the top of your file:

```json
"@plural": "french"
```

| Rule       | Uses `.one` when |
|------------|------------------|
| `english`  | count is 1       |
| `french`   | count is 0 or 1  |

If your language does not distinguish here, put the same text in both. If it needs a rule that
does not exist yet, add one to `PluralSuffix` in `Localization.cs` - it is a single `case`.

These are full sentences on purpose. Earlier versions built them by splicing a verb into a
template, which only ever worked for English; please keep each wording complete rather than
reintroducing a placeholder for a word.

## 6. Metadata keys

Any key starting with `@` is metadata, never displayed. `@plural` is the only one the mod reads,
but you can add your own notes:

```json
"@note": "Translated by ... - wording follows the French game client"
```

## 7. Check your work

The test suite verifies that every language file has the same keys as `en.json` and that no
placeholder went missing. Run it from the repository root:

```
dotnet test Tests/MoreCheckmarks.Tests.csproj
```
