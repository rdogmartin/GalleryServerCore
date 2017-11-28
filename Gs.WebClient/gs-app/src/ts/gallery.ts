$(document).ready(function () {
    console.log('gallery.ts: ready event fired');
});

module Gs {
    jQuery.fn.equalSize = function (wBuffer?: number, hBuffer?: number) {
        wBuffer = wBuffer || 0;
        hBuffer = hBuffer || 0;

        return (<JQuery>this).width(wBuffer + Math.max.apply(null,
            (<JQuery>this).map(function () {
                return jQuery(this).width();
            }).get()
        )).height(hBuffer + Math.max.apply(null,
            (<JQuery>this).map(function () {
                return jQuery(this).height();
            }).get()
        ));
    };
}

interface JQuery {
    //gsTooltip(options?: Gs.GsTooltipOptions): gsTooltip;
    //equalHeights(hBuffer?: number): equalHeights;
    //equalWidths(wBuffer?: number): equalWidths;
    equalSize(wBuffer?: number, hBuffer?: number): equalSize;
    //gsThumbnails(options?: Gs.GsThumbnailsOptions): gsThumbnails;
    //gsThumbnails(method?: string): void;
    //gsTreeView(data: any, options?: Gs.GsTreeViewOptions): gsTreeView;
    //gsMedia(options?: Gs.GsMediaOptions): gsMedia;
    //gsMedia(method?: string): JQuery;
    //plupload(settings: plupload_settings): JQuery;
    //plupload(id: string): plupload;
}


//interface equalHeights extends JQuery { }
//interface equalWidths extends JQuery { }
//interface gsTooltip extends JQuery { }
interface equalSize extends JQuery { }
//interface gsThumbnails extends JQueryUI.Widget { }
//interface gsTreeView extends JQueryUI.Widget { }
//interface gsMedia extends JQueryUI.Widget { }
