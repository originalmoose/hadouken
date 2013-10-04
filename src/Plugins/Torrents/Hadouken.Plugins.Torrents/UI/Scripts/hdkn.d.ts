﻿declare module Hadouken.Events {
    class EventListener {
        addHandler(name: string, callback: { (data: any): void; }): void;
        connect(): void;
        sendEvent(name: string, data: any): void;
    }
}

declare module Hadouken.UI {
    class Page {
        constructor(url: string);
        load(): void;
        setup(): void;

        getContent(): any;
    }

    class Dialog {
        constructor(url: string);
        show();
        onShow();

        getContent(): any;
    }
}

declare module Hadouken.Http {
    class JsonRpcClient {
        constructor(url: string);
        call(method: string, callback: { (result: any): void; }): void;
        callParams(method: string, params: any, callback: { (result: any): void; }): void;
    }
}