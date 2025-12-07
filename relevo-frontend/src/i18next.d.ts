import "i18next";

// Augment i18next to ensure t() returns string
declare module "i18next" {
  interface CustomTypeOptions {
    // Ensure t() always returns string, never null or empty string
    returnNull: false;
    returnEmptyString: false;
    // This tells i18next that we're using JSON resources
    jsonFormat: "v4";
    // Allow any namespace since we load dynamically
    allowObjectInHTMLChildren: false;
  }
}

// Ensure react-i18next uses the same configuration
declare module "react-i18next" {
  interface CustomTypeOptions {
    returnNull: false;
    returnEmptyString: false;
  }
}
