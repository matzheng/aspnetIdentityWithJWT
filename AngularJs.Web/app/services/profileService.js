'use strict';

app.factory('profileService', ['$http', 'nAuthSettings', function ($http, nAuthSettings) {
    var serviceBase = nAuthSettings.apiServiceBaseUri;

    var profileServiceFactory = {};
    var _getProfile = function () {
        return $http.get(serviceBase + "api/account/profile/ceshi1").then(function (result) {
            return result;
        });
    };

    profileServiceFactory.getProfile = _getProfile;

    return profileServiceFactory;
}]);