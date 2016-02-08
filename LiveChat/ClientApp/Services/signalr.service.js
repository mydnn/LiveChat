(function ($, angular, document) {
    'use strict';

    HubFactory.$inject = ['$rootScope', '$q', 'HubConnectionEvents'];
    function HubFactory($rootScope, $q, ConnectionEvents) {

        /**
		 * Class Hub
		 * This is a re-usable Class
		 * @example
		 * 
		 * var myHub = new Hub('myHub');
		 * 
		 * myHub.send('sendMessage', 'Hello from Client');
		 * 
		 * myHub.on('receiveMessage', function(msg){ console.log('Server:', msg); });
		 * 
		 * @param {String} hubName
		 * @param [{Object}] options
		 */
        function Hub(hubName, options) {

            // Ensure a hub name was passed
            if (!hubName) {
                throw new Error('Hub name was not specified, be sure to pass it in when invoking the Hub class');
            }

            // Hub Settings
            var settings = angular.extend({

                // Enable hub logging events
                loggingEnabled: false,

            }, options);

            // Create and set the connection property
            this.connection = $.hubConnection(settings.connectionPath);

            // Set Logging
            this.connection.logging = settings.loggingEnabled;

            // Create and set the Hub Proxy
            this.proxy = this.connection.createHubProxy(hubName);

            // Bind to connection events , this is only done once.
            bindConnectionEvents(this);


        }

        /**
		 * Add the following methods to the Hub.prototype chain
		 */
        Hub.prototype = {
            /**
			 * Starts the hub connection
			 * @return {Promise}
			 */
            start: function start() {
                return this.connection.start({ transport: 'longPolling' });
            },

            /**
			 * Hub.on
			 * @param {String} evt
			 * @param {Function} fn
			 */
            on: function on(evt, fn) {

                this.proxy.on(evt, function () {

                    var args = arguments;

                    // Have angular run a digest
                    $rootScope.$evalAsync(function () {
                        fn.apply(fn, args);
                    });

                });

                return this; // Return for chaining
            },

            /**
			 * Hub.off
			 * Stops listening to passed in event
			 * 
			 * @example
			 * Hub.off('getDataFromHub', getDataFromHubCallback);
			 */
            off: function () {
                this.proxy.off.apply(this.proxy, arguments);
            },

            /**
			 * Hub.invoke
			 * Method will ensure that a connection has been established before
			 *	calling to the hub
			 * 
			 * @example
			 * Hub.invoke('sendMessage', 'Message to Send');
			 */
            invoke: function invoke() {

                // Store the passed in arguments
                var args = arguments,

					// Send will always return a promise.
					// Promises are resolved by the hub invoke method
					deferred = $q.defer(),

					// Internal context
					self = this
                ;

                // Resolve the invoke call and it's promise
                function resolve() {
                    // Our promise is either resolved or rejected by the hubs response.
                    self.proxy.invoke.apply(self.proxy, args).then(deferred.resolve, deferred.reject);
                }

                // Resolve the method immediately if the connection is established
                if (this.connection.state === $.signalR.connectionState.connected) {
                    resolve();
                }

                // In the event that we're disconnected
                if (this.connection.state === $.signalR.connectionState.disconnected) {

                    // Start the connection, then resolve once we're connected
                    this.start().done(function () {
                        resolve();
                    });
                }

                // Return the promise
                return deferred.promise;
            },

            /**
			 * Alias for invoke
			 */
            send: function () {
                return this.invoke.apply(this, arguments);
            },

            /**
			 * Exposes the Hubs connection status
			 */
            connectionStatus: {

                // Disconnected flag
                disconnected: false,

                // Reconnecting flag
                reconnecting: false,

                /**
				 * Determine if the hub is connected
				 * @return boolean
				 */
                isConnected: function () {
                    return !this.disconnected && !this.reconnecting;
                },

                /**
				 * Is connection disconnected
				 */
                isDisconnected: function () {
                    return this.disconnected;
                },

                /**
				 * Is Reconnecting
				 */
                isReconnecting: function () {
                    return this.reconnecting;
                },

                /**
				 * Determine if the hub connection is down
				 * 
				 * @return boolean
				 */
                isDown: function () {
                    return this.disconnected || this.reconnecting;
                },

                /**
				 * Update the connection status
				 * @param {Boolean} reconnectVal
				 * @param {Boolean} disconnectVal
				 */
                setConnection: function (reconnectVal, disconnectVal) {

                    var self = this;

                    // Ask angular to udpate the digest
                    $rootScope.$evalAsync(function () {
                        self.reconnecting = reconnectVal;
                        self.disconnected = disconnectVal;
                    });
                }
            }
        };


        /**
		 * Bind to the connection events.
		 * This is a private method, we use it to bind the hubs connection events when constructed.
		 * 
		 * @param {Hub} hubInstance
		 */
        function bindConnectionEvents(hubInstance) {

            /**
			 * Uses rootScope to broadcast the desired connection event
			 */
            function broadcastConnectionEvent() {
                $rootScope.$broadcast.apply($rootScope, arguments);
            }

            /**
			 * Update the connection status on the hub, when the state changes
			 */
            function updateConnectionState(evt, state) {

                var reconnecting = state.newState === $.signalR.connectionState.reconnecting,
					disconnected = state.newState === $.signalR.connectionState.disconnected
                ;

                hubInstance.connectionStatus.setConnection(reconnecting, disconnected);

            }

            // Hook into the change event
            $rootScope.$on(ConnectionEvents.change, updateConnectionState);

            // Bind to the connection reconnecting event
            hubInstance.connection.reconnecting(function () {
                broadcastConnectionEvent(ConnectionEvents.reconnecting);
            });

            // Bind to the connection reconnected event
            hubInstance.connection.reconnected(function () {
                broadcastConnectionEvent(ConnectionEvents.reconnected);
            });

            // Bind to the connection disconnected event
            hubInstance.connection.disconnected(function () {
                broadcastConnectionEvent(ConnectionEvents.disconnected);
            });

            // Bind to the connection error event
            hubInstance.connection.error(function (error, data) {
                broadcastConnectionEvent(ConnectionEvents.error, error, data);
            });

            // Bind to the connection change event
            hubInstance.connection.stateChanged(function (state) {
                broadcastConnectionEvent(ConnectionEvents.change, state);
            });
        }



        // Return our Class
        return Hub;

    }

    angular.module('services.hub', [])
		// Hub Conncetion event object
		// This can be used throughout the application to hook into the $scope or $rootScope for connection events
		//
		.value('HubConnectionEvents', {
		    change: 'hub:connection:change',
		    error: 'hub:connection:error',
		    disconnected: 'hub:connection:disconnected',
		    reconnected: 'hub:connection:reconnected',
		    reconnecting: 'hub:connection:reconnecting'
		})

		// Register the Hub Proxy Service
		.factory('HubProxy', HubFactory);



})(window.jQuery, window.angular, document);
