'use strict';
app.controller('profileController', ['$scope' , 'profileService', function ($scope, profileService) {
    $scope.profile = [];

    profileService.getProfile().then(function (result) {
        $scope.profile = result;
    }, function (error) {

    });
}]);