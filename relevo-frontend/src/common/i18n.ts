import i18n, { type InitOptions } from "i18next";
import LanguageDetector from "i18next-browser-languagedetector";
import { initReactI18next } from "react-i18next";
import translationEN from "../assets/locales/en/translations.json";
import translationES from "../assets/locales/es/translations.json";
import { isProduction } from "./utils";

export const namespaces = [
	"actionList",
	"activityFeed",
	"appSidebar",
	"clinicalDocumentation",
	"collaborationPanel",
	"collapsibleLayout",
	"confirmationChecklist",
	"contextAwareDashboard",
	"dailySetup",
	"desktopPatientView",
	"enhancedLayout",
	"enhancedPatientCard",
	"figmaLayout",
	"fullscreenEditor",
	"handover",
	"handoverHistory",
	"header",
	"illnessSeverity",
	"justification",
	"mainContent",
	"mobileMenus",
	"modernLayout",
	"notificationsView",
	"patientAlerts",
	"patientCard",
	"patientDetailView",
	"patientListView",
	"patientSelectionCard",
	"patientSummary",
	"profileView",
	"quickActions",
	"quickNote",
	"searchBar",
	"searchView",
	"simpleLayout",
	"simplePatientCard",
	"situationAwareness",
	"statusSummary",
	"synthesisByReceiver",
	"patientHeader",
  ] as const;

export const defaultNS = "translations" as const;

export const resources = {
	en: translationEN,
	es: translationES,
} as const;

const i18nOptions: InitOptions = {
	resources,
	defaultNS,
	ns: namespaces,
	debug: !isProduction,
	lng: import.meta.env["VITE_APP_LANG"] as string || "es",
	supportedLngs: ["en", "es"],
	nonExplicitSupportedLngs: true,
	load: "languageOnly",
	fallbackLng: "es",
	detection: {
		order: ["navigator", "htmlTag"],
		caches: [],
	},
	interpolation: {
		escapeValue: false, // not needed for react as it escapes by default
	},
};

void i18n
	.use(LanguageDetector)
	.use(initReactI18next)
	.init(i18nOptions);
