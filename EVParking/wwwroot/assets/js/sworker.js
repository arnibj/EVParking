"use strict";
var programURL = "/"
var CACHE_NAME = 'cache-v2.9';
console.log('WORKER: executing...');
var version = 'v1:0:9';
var offlineFundamentals = ['/'];
self.addEventListener("install", function (event) {
    //console.log('WORKER: install event in progress.');
    event.waitUntil(
      caches
        .open(version + 'pages')
            .then(function (cache) {
                return cache.addAll(offlineFundamentals);
            })
        .then(function () {
            console.log('WORKER: install completed');
        })
    );
});
self.addEventListener("fetch", function (event) {
    if (event.request.method !== 'GET') { return; }
    if (event.request.url.toLowerCase().indexOf('.aspx') !== -1) { return; }
    event.respondWith(
      caches
        .match(event.request)
        .then(function (cached) {
            var networked = fetch(event.request)
              .then(fetchedFromNetwork, unableToResolve)
              .catch(unableToResolve);
            // console.log('WORKER: fetch event', cached ? '(cached)' : '(network)', event.request.url);

            return cached || networked;
            function fetchedFromNetwork(response) {
                var cacheCopy = response.clone();
                //console.log('WORKER: fetch response from network.', event.request.url);
                caches
                  .open(version + 'pages')
                  .then(function add(cache) {
                      return cache.put(event.request, cacheCopy);
                  })
                  .then(function () {
                      //console.log('WORKER: fetch response stored in cache.', event.request.url);
                  });
                return response;
            }
            function unableToResolve() {
                //console.log('WORKER: fetch request failed in both cache and network.');
                return new Response('<h1>Service Unavailable</h1>', {
                    status: 503,
                    statusText: 'Service Unavailable',
                    headers: new Headers({
                        'Content-Type': 'text/html'
                    })
                });
            }
        })
    );
});
self.addEventListener("activate", function (event) {
    //console.log('WORKER: activate event in progress.');
    event.waitUntil(
      caches
        .keys()
        .then(function (keys) {
            return Promise.all(
              keys
                .filter(function (key) {
                    return !key.startsWith(version);
                })
                .map(function (key) {
                    return caches.delete(key);
                })
            );
        })
        .then(function () {
            //console.log('WORKER: activate completed.');
        })
    );
});

self.addEventListener('push', function (event) {
    console.log('[Service Worker] Push Received.');
    //console.log(`[Service Worker] Push had this data: "${event.data}"`);
    var payload = JSON.parse(event.data.text());
    var title = payload.subject;
    const options = {
        body: payload.body,
        icon: payload.icon,
        tag: payload.tag
    };
    event.waitUntil(self.registration.showNotification(title, options));
});

self.addEventListener('notificationclick', function (event) {
    console.log('[Service Worker] Notification click Received.');
    event.notification.close();
    event.waitUntil(
      clients.openWindow(programURL + '/Notifications')
    );
});