export class NgxBootstrapLocaleMappingService {
    map(locale: string): string {
        const cultureMap = {
            'zh-Hans': 'zh-cn'
            // Add more here
        };

        if (cultureMap[locale]) {
            return cultureMap[locale];
        }

        return locale;
    }

    getModuleName(locale: string): string {
        const moduleNameMap = {
            'pt-BR': 'ptBr',
            'zh-Hans': 'zhCn'
            // Add more here
        };

        if (moduleNameMap[locale]) {
            return moduleNameMap[locale];
        }

        return locale;
    }
}
