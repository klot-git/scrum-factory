var SF = SF || {};

SF.ProjectSearch = function () {

    this.webapp = '';

    this.projects = new ko.observableArray();
    this.tagFilter = new ko.observable('');
    this.firstSearch = new ko.observable(false);

    var me = this;

    this.search = function () {
        $.ajax({
            url: me.webapp + 'projectsService/Projects?filter=OPEN_PROJECTS&top=20&tagFilter=' + me.tagFilter(),
            contentType: 'application/json; charset=utf-8',
            type: 'GET',
            success: function (data) {
                me.firstSearch(true);
                me.projects(data);
            },
            error: function (xhr, ajaxOptions, thrownError) {
                alert('erro');
            }
        });
    }

    this.shouldSearch = false;
    this.lastSearch = null;
    this.keyPressed = function () {

        if (me.shouldSearch) {
            me.shouldSearch = false;
            return;
        }

        me.shouldSearch = true;

        setTimeout(function () {
            if (me.shouldSearch) {
                me.shouldSearch = false;
                if (me.lastSearch != me.tagFilter())
                    me.search();
                me.lastSearch = me.tagFilter();
            }
            else
                me.keyPressed();
        }, 1000);
    }
}

var pageVM = new SF.ProjectSearch();
$(document).ready(function () {
    ko.applyBindings(pageVM);

});
