//var commentServiceURL = "http://localhost:56299/GetSuggestedFixes.svc/";
var commentServiceURL = "http://www.sitecoreloganalyzer.com/GetSuggestedFixes.svc/";
var VID = 'anonymous';
if(this.VoterID){VID=VoterID;}

/////////////webservice wrapper//////////////////////
function upvote(commentID, placeToUpdateExceptions) {
    $.ajax({
        type: "GET",
        dataType: "jsonp",
        url: commentServiceURL + "UpVoteComment?c=" + commentID + "&v=" + VID,
        success: function (data) { 
            if(data=="-1")
            {
                alert('We already have your vote on this comment');
            }else{
                getCommentsForException(placeToUpdateExceptions);
            }},
        error: function (data) { alert('We are having issues taking votes right now.  data: ' + data); }
    });
}
function downvote(commentID, placeToUpdateExceptions) {
    $.ajax({
        type: "GET",
        dataType: "jsonp",
        url: commentServiceURL + "DownVoteComment?c=" + commentID + "&v=" + VID,
        success: function (data) { 
            if(data=="-1")
            {
                alert('We already have your vote on this comment');
            }else{
                getCommentsForException(placeToUpdateExceptions);
            }},
        error: function (data) { alert('We are having issues taking votes right now.  data: ' + data); }
    });
}
function getCommentsForException(tableCellWithException) {
    $.ajax({
        type: "GET",
        url: commentServiceURL + "GetCommentsForException?w=" + encodeURI($(tableCellWithException).find(".litException").html()),
        contentType: "application/json; charset=utf-8",
        dataType: "jsonp",
        success: function (data) 
        { 
            if(data){
                if($(tableCellWithException).find(".commentsForException").length < 1)
                {
                    $(tableCellWithException).append('<div class="commentsForException"></div>');
                }
                var spot = $(tableCellWithException).find(".commentsForException");
                spot.html('');
                spot.append("<b>Comments:</b><br /><table>");
                for(x=0;x<data.length;x++){
                    spot.append('<tr><td>' + data[x].Votes + 
                        '</td><td>'
                        + '<img src="Styles/img/thumb_up.png" alt="upvote" class="upvote" cid="' + data[x].ID + '" /></a></td><td>'
                        + '<img src="Styles/img/thumb_down.png" alt="downvote" class="downvote" cid="' + data[x].ID + '" /></a></td><td>'
                        + unescape(data[x].Comment) + "</td></tr>");
                }
                spot.append("</table>");
                spot.slideDown();
            }
        },
        error: function (data) 
        { 
            alert('We are having an issue retreiving comments right now.  data: ' + data);
        }
    });
}
//// cannot do cross domain JSONP post
//function getCommentNumbersForExceptions(exceptions) {
//    $.ajax({
//        type: "POST",
//        url: commentServiceURL + "GetCommentNumbersForExceptions",
//        contentType: "application/json; charset=utf-8",
//        dataType: "jsonp",
//        data: JSON.stringify(exceptions),
//        success: function (data) { alert('we r gud: TODO: iterate and get data' + data); },
//        error: function (data) { alert('sumthin went rong: ' + data); }
//    });
//}
function postExceptionComment(exception, comment, placeToUpdateExceptions) {
    $.ajax({
        url: commentServiceURL + "PostExceptionComment?w=" + encodeURI(exception) + "&c=" + encodeURI(comment) + "&p=0",
        contentType: "application/json; charset=utf-8",
        dataType: "jsonp",
        success: function (data) { 
            if(placeToUpdateExceptions!=undefined){
                getCommentsForException(placeToUpdateExceptions);
            }
        },
        error: function (data) {
            alert('We are having issues taking comments right now.  data: ' + data); 
        }
    });
}
/////////////////////////////////////////////////////

$(document).ready(function () {
    $(".lnkException").click(function () {
        // expand out some comments
        getCommentsForException(this.parentNode);
    });
    $(".postComment").click(function () {
        var commentText = prompt("what's your comment");
        if (commentText.length > 0) {
            postExceptionComment($($(this).parents("tr")[0]).find(".litException").html(), commentText, $($(this).parents("tr")[0]).find("td.tdException")[0]);
        }
        return false;
    });
    $(".graphBar").click(function () {
        window.location = "ExceptionViewer.aspx?dt=" + encodeURI($(this).attr("dt"));
    });
    $(".upvote").live("click", function () {
        upvote($(this).attr("cid"), $(this).parents("td.tdException")[0]);
    });
    $(".downvote").live("click", function () {
        downvote($(this).attr("cid"), $(this).parents("td.tdException")[0]);
    });
});
