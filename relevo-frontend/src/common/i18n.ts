import i18n, { type InitOptions, type Resource, type ResourceLanguage } from "i18next";
import LanguageDetector from "i18next-browser-languagedetector";
import { initReactI18next } from "react-i18next";
import { isProduction } from "@/common/utils";

// Load all JSON locales at build time: /assets/locales/<lng>/<ns>.json
const files = import.meta.glob<{ default: ResourceLanguage }>("@/assets/locales/*/*.json", { eager: true });

// Build { en: { ns: {...}, ... }, es: { ... } }
const resources: Resource = {};

for (const [path, module_] of Object.entries(files)) {
  const match = path.match(/assets\/locales\/([^/]+)\/([^/]+)\.json$/);
  if (!match) continue;
  const lng = match[1]!;
  const ns = match[2]!;
  (resources[lng] ??= {} as ResourceLanguage)[ns] = module_.default; // each file is a namespace
}

// Default namespace
const defaultNS = "actionList";

const i18nOptions: InitOptions = {
  resources, // bundled resources
  defaultNS,
  lng: (import.meta.env["VITE_APP_LANG"] as string) || "es",
  fallbackLng: "es",
  supportedLngs: Object.keys(resources),
  nonExplicitSupportedLngs: true,
  load: "languageOnly",
  debug: !isProduction,
  detection: {
    order: ["navigator", "htmlTag"],
    caches: [],
  },
  interpolation: { escapeValue: false },
  react: { useSuspense: false }, // no suspense needed (eager load)
};

void i18n
  .use(LanguageDetector)
  .use(initReactI18next)
  .init(i18nOptions);

export default i18n;
