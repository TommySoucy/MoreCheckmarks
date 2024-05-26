const fs = require("node:fs");
const path = require("node:path");

// Taken from: https://hub.sp-tarkov.com/files/file/1022-gunsmith/
const PRESETS_FILE_PATH = path.join(__dirname, "gunsmith-presets.json");
const OUTPUT_FILE_PATH = path.join(__dirname, "../", "GunsmithItems.cs");

function main() {
  // Example:
  const presets = JSON.parse(fs.readFileSync(PRESETS_FILE_PATH, "utf8"));

  let generatedCode = `
using System.Collections.Generic;

namespace GunsmithItems
{
    public static class WeaponMods
    {
        public static readonly Dictionary<string, List<string>> mods = new Dictionary<string, List<string>>()
        {
`;

  for (const [key, value] of Object.entries(presets)) {
    const modItems = value.items
      .filter((it) => it.slotId != null)
      .map((it) => it._tpl);
    generatedCode += `
            {
                "${key}",
                new List<string> { ${modItems
                  .map((it) => `"${it}"`)
                  .join(", ")} }
            },
`;
  }

  generatedCode += `
        };
    }
}
`;

  fs.writeFileSync(OUTPUT_FILE_PATH, generatedCode);
  console.log(`Generated code written to ${OUTPUT_FILE_PATH}`);
}

main();
