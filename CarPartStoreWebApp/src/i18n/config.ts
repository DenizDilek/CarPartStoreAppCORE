import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import LanguageDetector from 'i18next-browser-languagedetector';

import en from './locales/en.json';
import tr from './locales/tr.json';

const resources = {
  en: {
    translation: en,
  },
  tr: {
    translation: tr,
  },
};

i18n
  // Detect user language
  .use(LanguageDetector)
  // Pass the i18n instance to react-i18next
  .use(initReactI18next)
  // Init i18next
  .init({
    resources,
    fallbackLng: 'en',
    debug: import.meta.env.DEV,

    interpolation: {
      escapeValue: false, // React already safes from XSS
    },

    detection: {
      // Order of language detection
      order: ['localStorage', 'navigator', 'htmlTag'],
      // Keys for localStorage
      lookupLocalStorage: 'i18nextLng',
      // Cache user language in localStorage
      caches: ['localStorage'],
      // Exclude cache from querystring
      excludeCacheFor: ['cimode'],
      // HTML attribute for language
      htmlTag: document.documentElement,
    },
  });

export default i18n;
