///<reference path="../node_modules/abp-web-resources/Abp/Framework/scripts/abp.d.ts"/>
///<reference path="../node_modules/abp-web-resources/Abp/Framework/scripts/libs/abp.jquery.d.ts"/>
///<reference path="../node_modules/abp-web-resources/Abp/Framework/scripts/libs/abp.signalr.d.ts"/>
///<reference path="../node_modules/moment/moment.d.ts"/>
///<reference path="../node_modules/@types/moment-timezone/index.d.ts"/>

// Typings reference file, see links for more information
// https://github.com/typings/typings
// https://www.typescriptlang.org/docs/handbook/writing-declaration-files.html

declare var System: any;

declare var mOffcanvas: any; // Related to Metronic
declare var mMenu: any; // Related to Metronic
declare var mToggle: any; // Related to Metronic
declare var mUtil: any; // Related to Metronic
declare var mHeader: any; // Related to Metronic

/**
 * rtl-detect
 */

declare module 'rtl-detect';

declare var App: any; //Related to Metronic
declare var Layout: any; //Related to Metronic

declare var Push: any;

interface JQuery {
    countTo(...any): any;
}

interface JQuery {
    sparkline(...any): any;
}

interface JQueryStatic {
    AdminBSB: any;
}