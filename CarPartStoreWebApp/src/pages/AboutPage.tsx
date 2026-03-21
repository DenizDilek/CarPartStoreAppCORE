import { useTranslation } from 'react-i18next';
import SEO from '@/components/seo/SEO';

export default function AboutPage() {
  const { t } = useTranslation();

  return (
    <div className="min-h-screen bg-background">
      <SEO
        title={t('about.title')}
        description={t('about.seoDescription')}
        url="/about"
      />
      {/* Hero Section */}
      <section className="bg-muted py-16">
        <div className="container mx-auto px-4 text-center">
          <h1 className="text-4xl md:text-5xl font-bold text-foreground mb-4">
            {t('about.heroTitle')}
          </h1>
          <p className="text-xl text-muted-foreground max-w-2xl mx-auto">
            {t('about.heroSubtitle')}
          </p>
        </div>
      </section>

      {/* About Section */}
      <section className="py-16">
        <div className="container mx-auto px-4">
          <div className="max-w-3xl mx-auto text-center">
            <h2 className="text-3xl font-bold text-foreground mb-6">{t('about.aboutTitle')}</h2>
            <p className="text-lg text-muted-foreground mb-4">
              {t('about.aboutText1')}
            </p>
            <p className="text-lg text-muted-foreground">
              {t('about.aboutText2')}
            </p>
          </div>
        </div>
      </section>

      {/* Service Cards - Placeholder */}
      <section className="py-16 bg-muted/50">
        <div className="container mx-auto px-4">
          <h2 className="text-3xl font-bold text-foreground text-center mb-12">{t('about.whyChooseUs')}</h2>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
            {/* Service Card 1 */}
            <div className="bg-card rounded-lg p-6 shadow-sm hover:shadow-md transition-shadow">
              <div className="text-4xl mb-4">✅</div>
              <h3 className="text-xl font-semibold mb-2">{t('about.qualityPartsTitle')}</h3>
              <p className="text-muted-foreground">
                {t('about.qualityPartsDesc')}
              </p>
            </div>

            {/* Service Card 2 */}
            <div className="bg-card rounded-lg p-6 shadow-sm hover:shadow-md transition-shadow">
              <div className="text-4xl mb-4">🚚</div>
              <h3 className="text-xl font-semibold mb-2">{t('about.fastDeliveryTitle')}</h3>
              <p className="text-muted-foreground">
                {t('about.fastDeliveryDesc')}
              </p>
            </div>

            {/* Service Card 3 */}
            <div className="bg-card rounded-lg p-6 shadow-sm hover:shadow-md transition-shadow">
              <div className="text-4xl mb-4">👨‍🔧</div>
              <h3 className="text-xl font-semibold mb-2">{t('about.expertSupportTitle')}</h3>
              <p className="text-muted-foreground">
                {t('about.expertSupportDesc')}
              </p>
            </div>
          </div>
        </div>
      </section>
    </div>
  );
}
