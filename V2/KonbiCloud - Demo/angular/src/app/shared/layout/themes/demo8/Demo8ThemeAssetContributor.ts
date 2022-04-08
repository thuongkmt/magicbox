import { IThemeAssetContributor } from '../ThemeAssetContributor';

export class Demo8ThemeAssetContributor implements IThemeAssetContributor {
    getAssetUrls(): string[] {
        return ['/assets/fonts/fonts-asap-condensed.css'];
    }
}
