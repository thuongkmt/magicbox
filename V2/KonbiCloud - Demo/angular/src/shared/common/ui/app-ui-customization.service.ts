import { Injectable } from '@angular/core';

@Injectable()
export class AppUiCustomizationService {

    getTheme() {
        return this.getSetting('App.UiManagement.Theme');
    }

    getThemeColor() {
        const theme = abp.setting.get('App.UiManagement.Theme').toLocaleLowerCase();
        if (theme === 'default') {
            return abp.setting.get('App.UiManagement.ThemeColor').toLocaleLowerCase();
        }

        return theme;
    }

    getContainerClass() {
        return this.getSetting('App.UiManagement.LayoutType') === 'boxed'
            ? 'm-container--responsive'
            : 'm-container--fluid';
    }

    getAsideSkin() {
        if (this.topMenuUsed() || this.tabMenuUsed()) {
            return this.getSetting('App.UiManagement.Header.Skin');
        }

        return this.getSetting('App.UiManagement.Left.AsideSkin');
    }

    allowAsideHiding() {
        return this.getSetting('App.UiManagement.Left.AllowAsideHiding') === 'true';
    }

    allowAsideMinimizing() {
        return this.getSetting('App.UiManagement.Left.AllowAsideMinimizing') === 'true';
    }

    leftMenuUsed(): boolean {
        return this.getSetting('App.UiManagement.Left.Position') === 'left';
    }

    topMenuUsed(): boolean {
        return this.getSetting('App.UiManagement.Left.Position') === 'top';
    }

    tabMenuUsed(): boolean {
        return this.getSetting('App.UiManagement.Left.Position') === 'tab';
    }

    isDesktopFixedHeader(): boolean {
        return this.getSetting('App.UiManagement.Header.DesktopFixedHeader') === 'true';
    }

    isMobileFixedHeader(): boolean {
        return this.getSetting('App.UiManagement.Header.MobileFixedHeader') === 'true';
    }

    getAppModuleBodyClass(): string {
        return 'm-page--' + this.getSetting('App.UiManagement.LayoutType') + ' m--skin-' + this.getSetting('App.UiManagement.ContentSkin') + ' ' +
            (this.getSetting('App.UiManagement.ContentSkin') !== '' ? ('m-content--skin-' + this.getSetting('App.UiManagement.ContentSkin')) : '') + ' ' +
            'm-header--' + (this.getSetting('App.UiManagement.Header.DesktopFixedHeader') === 'true' ? 'fixed' : 'static') + ' ' +
            (this.getSetting('App.UiManagement.Header.MobileFixedHeader') === 'true' ? 'm-header--fixed-mobile' : '') + ' ' +
            ((this.getSetting('App.UiManagement.Left.FixedAside') === 'true' && !this.topMenuUsed() ? 'm-aside-left--fixed' : '')) + ' ' +
            (this.getSetting('App.UiManagement.Left.DefaultMinimizedAside') === 'true' ? 'm-aside-left--minimize m-brand--minimize' : '') + ' ' +
            (this.getSetting('App.UiManagement.Left.DefaultHiddenAside') === 'true' || this.getSetting('App.UiManagement.Left.Position') === 'top' ? 'm-aside-left--hide' : '') + ' ' +
            'm-aside-left--enabled' + ' ' +
            'm-aside-left--skin-' + this.getSetting('App.UiManagement.Left.AsideSkin') + ' ' +
            'm-aside-left--offcanvas' + ' ' +
            (this.getSetting('App.UiManagement.Footer.FixedFooter') === 'true' && this.getSetting('App.UiManagement.LayoutType') !== 'boxed' ? 'm-footer--fixed' : '');
    }

    getAccountModuleBodyClass() {
        return 'm--skin- m-header--fixed m-header--fixed-mobile m-aside-left--enabled m-aside-left--skin-dark m-aside-left--offcanvas m-footer--push m-aside--offcanvas-default';
    }

    getSelectEditionBodyClass() {
        return 'm--skin-';
    }

    getHeaderSkin() {
        return this.getSetting('App.UiManagement.Header.Skin');
    }

    //User navigation menu
    getSideBarMenuClass(): string {
        let menuCssClass = 'm-aside-menu m-aside-menu--skin-' + this.getSetting('App.UiManagement.Left.AsideSkin');

        menuCssClass += ' m-aside-menu--submenu-skin-';
        menuCssClass += this.getSetting('App.UiManagement.Left.AsideSkin');

        return menuCssClass;
    }

    getMenuListClass(): string {
        return 'm-menu__nav--dropdown-submenu-arrow';
    }

    getTopBarMenuClass(): string {
        let menuCssClass = 'm-header-menu m-aside-header-menu-mobile m-aside-header-menu-mobile--offcanvas m-header-menu--skin-' + this.getSetting('App.UiManagement.Left.AsideSkin');
        menuCssClass += ' m-header-menu--submenu-skin-' + this.getSetting('App.UiManagement.Left.AsideSkin');
        menuCssClass += ' m-aside-header-menu-mobile--skin-' + this.getSetting('App.UiManagement.Left.AsideSkin');
        menuCssClass += ' m-aside-header-menu-mobile--submenu-skin-' + this.getSetting('App.UiManagement.Left.AsideSkin');

        if (this.getSetting('App.UiManagement.LayoutType') === 'boxed') {
            return menuCssClass + ' m-container--xxl';
        }

        return menuCssClass;
    }

    getTopBarMenuContainerClass(): string {
        //m-header__bottom m-header-menu--skin-light m-container--xxl m-container m-container--full-height m-container--responsive
        let menuCssClass = 'm-header__bottom m-header-menu--skin-' + this.getSetting('App.UiManagement.Left.AsideSkin') + ' m-container m-container--full-height m-container--responsive';
        if (this.getSetting('App.UiManagement.LayoutType') === 'boxed') {
            return menuCssClass + ' m-container--xxl';
        }

        return menuCssClass;
    }

    getIsMenuScrollable(): boolean {
        return this.getSetting('App.UiManagement.Left.FixedAside') === 'true';
    }

    private getSetting(key: string): string {
        let setting = abp.setting.get(key);
        if (!setting) {
            return null;
        }

        return abp.setting.get(key).toLocaleLowerCase();
    }
}
