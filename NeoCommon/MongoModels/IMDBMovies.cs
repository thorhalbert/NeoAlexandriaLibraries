using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;

namespace NeoCommon.MongoModels
{
    public static class IMDBMovies_exts
    {
        public static IMongoCollection<IMDBMovies> IMDBMovies(this IMongoDatabase db)
        {
            return db.GetCollection<IMDBMovies>("IMDBMovies");
        }
    }

    public class IMDBPerson
    {
        [BsonIgnoreIfNull] public string? id { get; set; }
        [BsonIgnoreIfNull] public string? name { get; set; }
        [BsonIgnoreIfNull] public string? notes { get; set; }

        [BsonIgnoreIfNull][BsonElement("current-role")] public IMDBCharacterNotes A_currentrole { get; set; }

        // Best to be sure:
        [BsonExtraElements] public BsonDocument _CatchAll { get; set; }
    }
    public class IMDBInfoPerson
    {
        [BsonIgnoreIfNull] public string? infoset { get; set; }
        [BsonIgnoreIfNull] public string? key { get; set; }
        [BsonIgnoreIfNull] public IMDBPerson person { get; set; }
    }
    public class IMDBInfoKeyPersonList
    {
        [BsonIgnoreIfNull] public string? infoset { get; set; }
        [BsonIgnoreIfNull] public string? key { get; set; }
        [BsonIgnoreIfNull] public List<IMDBPerson> person { get; set; }
    }

    public class IMDBCompanyList
    {
        [BsonIgnoreIfNull] public List<IMDBPerson> company { get; set; }
        [BsonIgnoreIfNull] public string? infoset { get; set; }
        [BsonIgnoreIfNull] public string? key { get; set; }
    }
    public class IMDBCompany
    {
        [BsonIgnoreIfNull] public IMDBPerson company { get; set; }
        [BsonIgnoreIfNull] public string? infoset { get; set; }
        [BsonIgnoreIfNull] public string? key { get; set; }
    }
    public class IMDBContent
    {
        internal IMDBContent(BsonValue bsonValue)
        {
            //full-size-cover-url = { "key" : "full-size cover url", "content" : "https://m.media-amazon.com/images/M/MV5BNzcxYTYyZGYtNjM3Yi00ODlkLWI3NmUtY2RiYjE1MjhjZDA5XkEyXkFqcGdeQXVyMTIxMDUyOTI@.jpg" }

            var doc = bsonValue.ToBsonDocument();

            string[] fields = { "infoset", "key", "content", "type", "item", "notes" };

            foreach (var f in doc.Names)
                switch (f)
                {
                    case "infoset":
                        infoset = doc[f].ToString();
                        break;
                    case "key":
                        key = doc[f].ToString();
                        break;
                    case "content":
                        content = doc[f].ToString();
                        break;
                    case "type":
                        type = doc[f].ToString();
                        break;
                    case "item":
                        item = doc[f].ToString();
                        break;
                    case "notes":
                        notes = doc[f].ToString();
                        break;
                }
        }

        [BsonIgnoreIfNull] public string? infoset { get; set; }
        [BsonIgnoreIfNull] public string? key { get; set; }
        [BsonIgnoreIfNull] public string? content { get; set; }
        [BsonIgnoreIfNull] public string? type { get; set; }
        [BsonIgnoreIfNull] public string? item { get; set; }
        [BsonIgnoreIfNull] public string? notes { get; set; }
    }
    public class IMDBCharacterNotes
    {
        [BsonIgnoreIfNull] public IMDBPerson character { get; set; }
        [BsonIgnoreIfNull] public string? notes { get; set; }
    }
    public class IMDBItemList
    {
        [BsonIgnoreIfNull] public string? infoset { get; set; }
        [BsonIgnoreIfNull] public List<string?> item { get; set; }
    }
    public class IMDBEffectCompanyPersonList
    {
        [BsonIgnoreIfNull] public string? infoset { get; set; }
        [BsonIgnoreIfNull] public string? key { get; set; }
        [BsonIgnoreIfNull] public List<IMDBPerson> person { get; set; }
        [BsonIgnoreIfNull] public IMDBPerson company { get; set; }
    }
    public class IMDBBoxOffice
    {
        [BsonIgnoreIfNull] public string? infoset { get; set; }
        [BsonIgnoreIfNull] public string? key { get; set; }
        [BsonIgnoreIfNull] public IMDBContent budget { get; set; }
        [BsonIgnoreIfNull][BsonElement("opening-weekend-united-states")] public IMDBContent A_openingweekendunitedstates { get; set; }
        [BsonIgnoreIfNull][BsonElement("cumulative-worldwide-gross")] public IMDBContent A_cumulativeworldwidegross { get; set; }
        [BsonIgnoreIfNull][BsonElement("opening-weekend-united-kingdom")] public IMDBContent A_openingweekendunitedkingdom { get; set; }
        [BsonIgnoreIfNull][BsonElement("opening-weekend-india")] public IMDBContent A_openingweekendindia { get; set; }
        [BsonIgnoreIfNull][BsonElement("opening-weekend-france")] public IMDBContent A_openingweekendfrance { get; set; }
        [BsonIgnoreIfNull][BsonElement("opening-weekend-italy")] public IMDBContent A_openingweekenditaly { get; set; }
        [BsonIgnoreIfNull][BsonElement("opening-weekend-spain")] public IMDBContent A_openingweekendspain { get; set; }
        [BsonIgnoreIfNull][BsonElement("opening-weekend-australia")] public IMDBContent A_openingweekendaustralia { get; set; }
        [BsonIgnoreIfNull][BsonElement("opening-weekend-japan")] public IMDBContent A_openingweekendjapan { get; set; }
        [BsonIgnoreIfNull][BsonElement("opening-weekend-netherlands")] public IMDBContent A_openingweekendnetherlands { get; set; }
        [BsonIgnoreIfNull][BsonElement("opening-weekend-germany")] public IMDBContent A_openingweekendgermany { get; set; }
        [BsonIgnoreIfNull][BsonElement("opening-weekend-hong-kong")] public IMDBContent A_openingweekendhongkong { get; set; }
        [BsonIgnoreIfNull][BsonElement("opening-weekend-turkey")] public IMDBContent A_openingweekendturkey { get; set; }
        [BsonIgnoreIfNull][BsonElement("opening-weekend-canada")] public IMDBContent A_openingweekendcanada { get; set; }
        [BsonIgnoreIfNull][BsonElement("opening-weekend-china")] public IMDBContent A_openingweekendchina { get; set; }
        [BsonIgnoreIfNull][BsonElement("opening-weekend-russia")] public IMDBContent A_openingweekendrussia { get; set; }
        [BsonIgnoreIfNull][BsonElement("opening-weekend-new-zealand")] public IMDBContent A_openingweekendnewzealand { get; set; }
        [BsonIgnoreIfNull][BsonElement("opening-weekend-brazil")] public IMDBContent A_openingweekendbrazil { get; set; }
        [BsonIgnoreIfNull][BsonElement("opening-weekend-south-korea")] public IMDBContent A_openingweekendsouthkorea { get; set; }
        [BsonIgnoreIfNull][BsonElement("opening-weekend-philippines")] public IMDBContent A_openingweekendphilippines { get; set; }
        [BsonIgnoreIfNull][BsonElement("opening-weekend-mexico")] public IMDBContent A_openingweekendmexico { get; set; }
        [BsonIgnoreIfNull][BsonElement("opening-weekend-south-africa")] public IMDBContent A_openingweekendsouthafrica { get; set; }
        [BsonIgnoreIfNull][BsonElement("opening-weekend-argentina")] public IMDBContent A_openingweekendargentina { get; set; }
        [BsonIgnoreIfNull][BsonElement("opening-weekend-belgium")] public IMDBContent A_openingweekendbelgium { get; set; }
        [BsonIgnoreIfNull][BsonElement("opening-weekend-denmark")] public IMDBContent A_openingweekenddenmark { get; set; }
        [BsonIgnoreIfNull][BsonElement("opening-weekend-sweden")] public IMDBContent A_openingweekendsweden { get; set; }
        [BsonIgnoreIfNull][BsonElement("opening-weekend-czechia")] public IMDBContent A_openingweekendczechia { get; set; }
        [BsonIgnoreIfNull][BsonElement("opening-weekend-indonesia")] public IMDBContent A_openingweekendindonesia { get; set; }
        [BsonIgnoreIfNull][BsonElement("opening-weekend-norway")] public IMDBContent A_openingweekendnorway { get; set; }
        [BsonIgnoreIfNull][BsonElement("opening-weekend-ireland")] public IMDBContent A_openingweekendireland { get; set; }
        [BsonIgnoreIfNull][BsonElement("opening-weekend-pakistan")] public IMDBContent A_openingweekendpakistan { get; set; }
        [BsonIgnoreIfNull][BsonElement("opening-weekend-taiwan")] public IMDBContent A_openingweekendtaiwan { get; set; }
        [BsonIgnoreIfNull][BsonElement("opening-weekend-thailand")] public IMDBContent A_openingweekendthailand { get; set; }
        [BsonIgnoreIfNull][BsonElement("opening-weekend-bulgaria")] public IMDBContent A_openingweekendbulgaria { get; set; }
        [BsonIgnoreIfNull][BsonElement("opening-weekend-finland")] public IMDBContent A_openingweekendfinland { get; set; }
        [BsonIgnoreIfNull][BsonElement("opening-weekend-greece")] public IMDBContent A_openingweekendgreece { get; set; }
        [BsonIgnoreIfNull][BsonElement("opening-weekend-hungary")] public IMDBContent A_openingweekendhungary { get; set; }
        [BsonIgnoreIfNull][BsonElement("opening-weekend-iran")] public IMDBContent A_openingweekendiran { get; set; }
        [BsonIgnoreIfNull][BsonElement("opening-weekend-malaysia")] public IMDBContent A_openingweekendmalaysia { get; set; }
        [BsonIgnoreIfNull][BsonElement("opening-weekend-nigeria")] public IMDBContent A_openingweekendnigeria { get; set; }
        [BsonIgnoreIfNull][BsonElement("opening-weekend-poland")] public IMDBContent A_openingweekendpoland { get; set; }
        [BsonIgnoreIfNull][BsonElement("opening-weekend-portugal")] public IMDBContent A_openingweekendportugal { get; set; }
        [BsonIgnoreIfNull][BsonElement("opening-weekend-romania")] public IMDBContent A_openingweekendromania { get; set; }
        [BsonIgnoreIfNull][BsonElement("opening-weekend-singapore")] public IMDBContent A_openingweekendsingapore { get; set; }
        [BsonIgnoreIfNull][BsonElement("opening-weekend-switzerland")] public IMDBContent A_openingweekendswitzerland { get; set; }
        [BsonIgnoreIfNull][BsonElement("opening-weekend-united-arab-emirates")] public IMDBContent A_openingweekendunitedarabemirates { get; set; }
        [BsonIgnoreIfNull][BsonElement("opening-weekend-venezuela")] public IMDBContent A_openingweekendvenezuela { get; set; }
        [BsonIgnoreIfNull][BsonElement("opening-weekend-yemen")] public IMDBContent A_openingweekendyemen { get; set; }
    }

    public class IMDBTitle
    {
        [BsonIgnoreIfNull] public string? id { get; set; }
        [BsonIgnoreIfNull] public string? title { get; set; }
    }
    public class IMDBEpisode
    {
        [BsonIgnoreIfNull] public string? infoset { get; set; }
        [BsonIgnoreIfNull] public string? key { get; set; }
        [BsonIgnoreIfNull] public IMDBTitle movie { get; set; }
    }


    public class IMDBMovieElements
    { 
        [BsonIgnoreIfNull][BsonElement("access-system")] public string? A_accesssystem { get; set; }
        [BsonIgnoreIfNull] public string? id { get; set; }
        //[BsonIgnoreIfNull][BsonElement("canonical-title")] public IMDBContent A_canonicaltitle { get; set; }
        //[BsonIgnoreIfNull][BsonElement("long-imdb-canonical-title")] public IMDBContent A_longimdbcanonicaltitle { get; set; }
        //[BsonIgnoreIfNull][BsonElement("long-imdb-title")] public IMDBContent A_longimdbtitle { get; set; }
        //[BsonIgnoreIfNull][BsonElement("smart-canonical-title")] public IMDBContent A_smartcanonicaltitle { get; set; }
        //[BsonIgnoreIfNull][BsonElement("smart-long-imdb-canonical-title")] public IMDBContent A_smartlongimdbcanonicaltitle { get; set; }
        //[BsonIgnoreIfNull] public IMDBContent title { get; set; }
        //[BsonIgnoreIfNull] public IMDBContent kind { get; set; }
        //[BsonIgnoreIfNull] public IMDBContent year { get; set; }
        //[BsonIgnoreIfNull] public IMDBContent genres { get; set; }
        //[BsonIgnoreIfNull] public IMDBInfoKeyPersonList cast { get; set; }
        //[BsonIgnoreIfNull] public IMDBContent countries { get; set; }
        //[BsonIgnoreIfNull][BsonElement("country-codes")] public IMDBContent A_countrycodes { get; set; }
        //[BsonIgnoreIfNull][BsonElement("language-codes")] public IMDBContent A_languagecodes { get; set; }
        //[BsonIgnoreIfNull] public List<IMDBContent> languages { get; set; }
        //[BsonIgnoreIfNull][BsonElement("color-info")] public IMDBContent A_colorinfo { get; set; }
        //[BsonIgnoreIfNull] public IMDBContent rating { get; set; }
        //[BsonIgnoreIfNull] public IMDBContent votes { get; set; }
        //[BsonIgnoreIfNull][BsonElement("cover-url")] public IMDBContent A_coverurl { get; set; }
        //[BsonIgnoreIfNull][BsonElement("full-size-cover-url")] public IMDBContent A_fullsizecoverurl { get; set; }
        //[BsonIgnoreIfNull][BsonElement("production-companies")] public IMDBCompanyList A_productioncompanies { get; set; }
        //[BsonIgnoreIfNull] public IMDBContent runtimes { get; set; }
        //[BsonIgnoreIfNull] public IMDBCompanyList distributors { get; set; }
        //[BsonIgnoreIfNull] public IMDBInfoPerson writer { get; set; }
        //[BsonIgnoreIfNull] public IMDBInfoPerson director { get; set; }
        //[BsonIgnoreIfNull] public IMDBItemList certificates { get; set; }
        //[BsonIgnoreIfNull] public IMDBItemList akas { get; set; }
        //[BsonIgnoreIfNull] public IMDBContent imdbid { get; set; }
        //[BsonIgnoreIfNull][BsonElement("localized-title")] public IMDBContent A_localizedtitle { get; set; }
        //[BsonIgnoreIfNull][BsonElement("original-title")] public IMDBContent A_originaltitle { get; set; }
        //[BsonIgnoreIfNull] public IMDBInfoPerson producer { get; set; }
        //[BsonIgnoreIfNull] public IMDBInfoPerson editor { get; set; }
        //[BsonIgnoreIfNull] public IMDBInfoPerson cinematographer { get; set; }
        //[BsonIgnoreIfNull][BsonElement("sound-crew")] public IMDBInfoKeyPersonList A_soundcrew { get; set; }
        //[BsonIgnoreIfNull][BsonElement("plot-outline")] public IMDBContent A_plotoutline { get; set; }
        //[BsonIgnoreIfNull][BsonElement("miscellaneous-crew")] public IMDBInfoKeyPersonList A_miscellaneouscrew { get; set; }
        //[BsonIgnoreIfNull][BsonElement("camera-and-electrical-department")] public IMDBInfoKeyPersonList A_cameraandelectricaldepartment { get; set; }
        //[BsonIgnoreIfNull][BsonElement("original-air-date")] public IMDBContent A_originalairdate { get; set; }
        //[BsonIgnoreIfNull][BsonElement("aspect-ratio")] public IMDBContent A_aspectratio { get; set; }
        //[BsonIgnoreIfNull][BsonElement("assistant-director")] public IMDBInfoKeyPersonList A_assistantdirector { get; set; }
        //[BsonIgnoreIfNull][BsonElement("editorial-department")] public IMDBInfoKeyPersonList A_editorialdepartment { get; set; }
        //[BsonIgnoreIfNull][BsonElement("make-up")] public IMDBInfoKeyPersonList A_makeup { get; set; }
        //[BsonIgnoreIfNull][BsonElement("sound-mix")] public IMDBContent A_soundmix { get; set; }
        //[BsonIgnoreIfNull][BsonElement("production-manager")] public IMDBInfoKeyPersonList A_productionmanager { get; set; }
        //[BsonIgnoreIfNull][BsonElement("music-department")] public IMDBInfoKeyPersonList A_musicdepartment { get; set; }
        //[BsonIgnoreIfNull] public IMDBInfoPerson composer { get; set; }
        //[BsonIgnoreIfNull][BsonElement("art-department")] public IMDBInfoKeyPersonList A_artdepartment { get; set; }
        //[BsonIgnoreIfNull][BsonElement("other-companies")] public IMDBCompanyList A_othercompanies { get; set; }
        //[BsonIgnoreIfNull] public IMDBItemList videos { get; set; }
        //[BsonIgnoreIfNull][BsonElement("costume-designer")] public IMDBInfoPerson A_costumedesigner { get; set; }
        //[BsonIgnoreIfNull][BsonElement("production-design")] public IMDBInfoPerson A_productiondesign { get; set; }
        //[BsonIgnoreIfNull][BsonElement("costume-department")] public IMDBInfoPerson A_costumedepartment { get; set; }
        //[BsonIgnoreIfNull][BsonElement("art-direction")] public IMDBInfoPerson A_artdirection { get; set; }
        //[BsonIgnoreIfNull][BsonElement("visual-effects")] public IMDBInfoKeyPersonList A_visualeffects { get; set; }
        //[BsonIgnoreIfNull][BsonElement("casting-director")] public IMDBInfoPerson A_castingdirector { get; set; }
        //[BsonIgnoreIfNull][BsonElement("stunt-performer")] public IMDBInfoKeyPersonList A_stuntperformer { get; set; }
        //[BsonIgnoreIfNull][BsonElement("script-department")] public IMDBInfoKeyPersonList A_scriptdepartment { get; set; }
        //[BsonIgnoreIfNull][BsonElement("special-effects")] public IMDBEffectCompanyPersonList A_specialeffects { get; set; }
        //[BsonIgnoreIfNull][BsonElement("location-management")] public IMDBInfoKeyPersonList A_locationmanagement { get; set; }
        //[BsonIgnoreIfNull][BsonElement("casting-department")] public IMDBInfoKeyPersonList A_castingdepartment { get; set; }
        //[BsonIgnoreIfNull][BsonElement("set-decoration")] public IMDBInfoPerson A_setdecoration { get; set; }
        //[BsonIgnoreIfNull][BsonElement("transportation-department")] public IMDBInfoKeyPersonList A_transportationdepartment { get; set; }
        //[BsonIgnoreIfNull] public IMDBInfoPerson thanks { get; set; }
        //[BsonIgnoreIfNull][BsonElement("box-office")] public IMDBBoxOffice A_boxoffice { get; set; }
        //[BsonIgnoreIfNull] public IMDBContent plot { get; set; }
        //[BsonIgnoreIfNull][BsonElement("number-of-seasons")] public List<IMDBContent> A_numberofseasons { get; set; }
        //[BsonIgnoreIfNull][BsonElement("series-years")] public IMDBContent A_seriesyears { get; set; }
        //[BsonIgnoreIfNull][BsonElement("special-effects-companies")] public IMDBCompany A_specialeffectscompanies { get; set; }
        //[BsonIgnoreIfNull][BsonElement("original-music")] public IMDBInfoPerson A_originalmusic { get; set; }
        //[BsonIgnoreIfNull][BsonElement("animation-department")] public IMDBInfoKeyPersonList A_animationdepartment { get; set; }
        //[BsonIgnoreIfNull][BsonElement("miscellaneous-companies")] public IMDBCompanyList A_miscellaneouscompanies { get; set; }
        //[BsonIgnoreIfNull][BsonElement("special-effects-department")] public IMDBInfoKeyPersonList A_specialeffectsdepartment { get; set; }
        //[BsonIgnoreIfNull][BsonElement("number-of-episodes")] public IMDBContent A_numberofepisodes { get; set; }
        //[BsonIgnoreIfNull] public IMDBContent episode { get; set; }
        //[BsonIgnoreIfNull] public IMDBContent season { get; set; }
        //[BsonIgnoreIfNull][BsonElement("canonical-episode-title")] public IMDBContent A_canonicalepisodetitle { get; set; }
        //[BsonIgnoreIfNull][BsonElement("canonical-series-title")] public IMDBContent A_canonicalseriestitle { get; set; }
        //[BsonIgnoreIfNull][BsonElement("episode-of")] public IMDBEpisode A_episodeof { get; set; }
        //[BsonIgnoreIfNull][BsonElement("episode-title")] public IMDBContent A_episodetitle { get; set; }
        //[BsonIgnoreIfNull][BsonElement("long-imdb-episode-title")] public IMDBContent A_longimdbepisodetitle { get; set; }
        //[BsonIgnoreIfNull][BsonElement("series-title")] public IMDBContent A_seriestitle { get; set; }
        //[BsonIgnoreIfNull][BsonElement("smart-canonical-episode-title")] public IMDBContent A_smartcanonicalepisodetitle { get; set; }
        //[BsonIgnoreIfNull][BsonElement("smart-canonical-series-title")] public IMDBContent A_smartcanonicalseriestitle { get; set; }
        //[BsonIgnoreIfNull][BsonElement("next-episode")] public IMDBContent A_nextepisode { get; set; }
        //[BsonIgnoreIfNull][BsonElement("previous-episode")] public IMDBContent A_previousepisode { get; set; }
        //[BsonIgnoreIfNull] public IMDBContent mpaa { get; set; }
        //[BsonIgnoreIfNull] public IMDBInfoPerson creator { get; set; }
        //[BsonIgnoreIfNull] public IMDBInfoPerson producers { get; set; }
        //[BsonIgnoreIfNull] public IMDBInfoPerson directors { get; set; }
        //[BsonIgnoreIfNull] public IMDBInfoKeyPersonList writers { get; set; }
        //[BsonIgnoreIfNull][BsonElement("sound-department")] public IMDBInfoPerson A_sounddepartment { get; set; }
        //[BsonIgnoreIfNull] public IMDBInfoKeyPersonList miscellaneous { get; set; }
        //[BsonIgnoreIfNull] public IMDBInfoPerson editors { get; set; }
        //[BsonIgnoreIfNull][BsonElement("camera-department")] public IMDBInfoKeyPersonList A_cameradepartment { get; set; }
        //[BsonIgnoreIfNull] public IMDBInfoPerson cinematographers { get; set; }
        //[BsonIgnoreIfNull] public IMDBInfoPerson composers { get; set; }
        //[BsonIgnoreIfNull][BsonElement("production-status")] public IMDBContent A_productionstatus { get; set; }
        //[BsonIgnoreIfNull][BsonElement("production-status-updated")] public IMDBContent A_productionstatusupdated { get; set; }
        //[BsonIgnoreIfNull][BsonElement("production-managers")] public IMDBInfoPerson A_productionmanagers { get; set; }
        //[BsonIgnoreIfNull][BsonElement("make-up-department")] public IMDBInfoKeyPersonList A_makeupdepartment { get; set; }
        //[BsonIgnoreIfNull][BsonElement("assistant-directors")] public IMDBInfoKeyPersonList A_assistantdirectors { get; set; }
        //[BsonIgnoreIfNull][BsonElement("costume-departmen")] public IMDBInfoKeyPersonList A_costumedepartmen { get; set; }
        //[BsonIgnoreIfNull][BsonElement("production-designers")] public IMDBInfoPerson A_productiondesigners { get; set; }
        //[BsonIgnoreIfNull][BsonElement("costume-designers")] public IMDBInfoPerson A_costumedesigners { get; set; }
        //[BsonIgnoreIfNull][BsonElement("casting-directors")] public IMDBInfoKeyPersonList A_castingdirectors { get; set; }
        //[BsonIgnoreIfNull][BsonElement("art-directors")] public IMDBInfoPerson A_artdirectors { get; set; }
        //[BsonIgnoreIfNull] public IMDBInfoKeyPersonList stunts { get; set; }
        //[BsonIgnoreIfNull][BsonElement("set-decorators")] public IMDBInfoPerson A_setdecorators { get; set; }
        //[BsonIgnoreIfNull][BsonElement("production-note")] public IMDBContent A_productionnote { get; set; }
        //[BsonIgnoreIfNull] public IMDBContent imdbindex { get; set; }
        //[BsonIgnoreIfNull][BsonElement("series-writers")] public IMDBInfoKeyPersonList A_serieswriters { get; set; }
        //[BsonIgnoreIfNull][BsonElement("top-250-rank")] public IMDBContent A_top250rank { get; set; }
        //[BsonIgnoreIfNull][BsonElement("series-miscellaneous")] public IMDBInfoKeyPersonList A_seriesmiscellaneous { get; set; }
        //[BsonIgnoreIfNull][BsonElement("series-art-department")] public IMDBInfoKeyPersonList A_seriesartdepartment { get; set; }
        //[BsonIgnoreIfNull][BsonElement("series-producers")] public IMDBInfoPerson A_seriesproducers { get; set; }
        //[BsonIgnoreIfNull][BsonElement("series-sound-department")] public IMDBInfoKeyPersonList A_seriessounddepartment { get; set; }
        //[BsonIgnoreIfNull][BsonElement("series-camera-department")] public IMDBInfoKeyPersonList A_seriescameradepartment { get; set; }
        //[BsonIgnoreIfNull][BsonElement("series-stunts")] public IMDBInfoKeyPersonList A_seriesstunts { get; set; }
        //[BsonIgnoreIfNull][BsonElement("series-visual-effects-department")] public IMDBInfoKeyPersonList A_seriesvisualeffectsdepartment { get; set; }
        //[BsonIgnoreIfNull][BsonElement("series-music-department")] public IMDBInfoKeyPersonList A_seriesmusicdepartment { get; set; }
        //[BsonIgnoreIfNull][BsonElement("series-assistant-directors")] public IMDBInfoKeyPersonList A_seriesassistantdirectors { get; set; }
        //[BsonIgnoreIfNull][BsonElement("series-editorial-department")] public IMDBInfoKeyPersonList A_serieseditorialdepartment { get; set; }
        //[BsonIgnoreIfNull][BsonElement("bottom-100-rank")] public IMDBContent A_bottom100rank { get; set; }
        //[BsonIgnoreIfNull][BsonElement("series-special-effects-department")] public IMDBInfoKeyPersonList A_seriesspecialeffectsdepartment { get; set; }
        //[BsonIgnoreIfNull][BsonElement("series-make-up-department")] public IMDBInfoKeyPersonList A_seriesmakeupdepartment { get; set; }
        //[BsonIgnoreIfNull][BsonElement("series-editors")] public IMDBInfoPerson A_serieseditors { get; set; }
        //[BsonIgnoreIfNull][BsonElement("series-directors")] public IMDBInfoPerson A_seriesdirectors { get; set; }
        //[BsonIgnoreIfNull][BsonElement("series-cinematographers")] public IMDBInfoPerson A_seriescinematographers { get; set; }
        //[BsonIgnoreIfNull][BsonElement("production-comments")] public IMDBContent A_productioncomments { get; set; }
        //[BsonIgnoreIfNull][BsonElement("series-costume-department")] public IMDBInfoPerson A_seriescostumedepartment { get; set; }
        //[BsonIgnoreIfNull][BsonElement("series-production-designers")] public IMDBInfoPerson A_seriesproductiondesigners { get; set; }
        //[BsonIgnoreIfNull] public IMDBContent synopsis { get; set; }
        //[BsonIgnoreIfNull][BsonElement("series-music-original")] public IMDBInfoPerson A_seriesmusicoriginal { get; set; }
        //[BsonIgnoreIfNull][BsonElement("series-casting-directors")] public IMDBInfoKeyPersonList A_seriescastingdirectors { get; set; }
        //[BsonIgnoreIfNull][BsonElement("series-transportation-department")] public IMDBInfoPerson A_seriestransportationdepartment { get; set; }
        //[BsonIgnoreIfNull][BsonElement("series-production-managers")] public IMDBInfoPerson A_seriesproductionmanagers { get; set; }
        //[BsonIgnoreIfNull][BsonElement("series-location-management")] public IMDBInfoKeyPersonList A_serieslocationmanagement { get; set; }
        //[BsonIgnoreIfNull][BsonElement("production-department")] public IMDBInfoPerson A_productiondepartment { get; set; }
        //[BsonIgnoreIfNull][BsonElement("series-art-directors")] public IMDBInfoPerson A_seriesartdirectors { get; set; }
        //[BsonIgnoreIfNull][BsonElement("series-casting-department")] public IMDBInfoKeyPersonList A_seriescastingdepartment { get; set; }
        //[BsonIgnoreIfNull][BsonElement("series-costume-designers")] public IMDBInfoPerson A_seriescostumedesigners { get; set; }
        //[BsonIgnoreIfNull][BsonElement("series-animation-department")] public IMDBInfoPerson A_seriesanimationdepartment { get; set; }
        //[BsonIgnoreIfNull][BsonElement("series-set-decorators")] public IMDBInfoPerson A_seriessetdecorators { get; set; }
        //[BsonIgnoreIfNull][BsonElement("music-non-original")] public IMDBInfoPerson A_musicnonoriginal { get; set; }
        //[BsonIgnoreIfNull][BsonElement("electrical-department")] public IMDBInfoPerson A_electricaldepartment { get; set; }
        //[BsonIgnoreIfNull][BsonElement("series-thanks")] public IMDBInfoKeyPersonList A_seriesthanks { get; set; }

        [BsonIgnore] public IMDBContent? full_size_cover_url {  get { return this._IMDBContentValue("full-size-cover-url");  } }

      


        // Best to be sure:
        [BsonExtraElements] public BsonDocument _CatchAll { get; set; }

        private IMDBContent? _IMDBContentValue(string key)
        {
            //full-size-cover-url = { "key" : "full-size cover url", "content" : "https://m.media-amazon.com/images/M/MV5BNzcxYTYyZGYtNjM3Yi00ODlkLWI3NmUtY2RiYjE1MjhjZDA5XkEyXkFqcGdeQXVyMTIxMDUyOTI@.jpg" }

            if (!_CatchAll.Contains(key)) return null;

            return new IMDBContent(_CatchAll[key]);
        }
    }




    public class IMDBMovies_MarkWhy
    {
        [BsonIgnoreIfNull] public List<IMDBPerson> cast { get; set; }
        [BsonIgnoreIfNull] public List<string?> Keywords { get; set; }
        [BsonIgnoreIfNull] public List<string?> TorKeywords { get; set; }
        [BsonIgnoreIfNull] public List<IMDBPerson> producer { get; set; }
        [BsonIgnoreIfNull] public List<IMDBPerson> director { get; set; }
        [BsonIgnoreIfNull] public string? Rating { get; set; }
        [BsonIgnoreIfNull] public List<IMDBPerson> writer { get; set; }
    }
    public class IMDBMovies
    {
        public string _id { get; set; }
        [BsonRequired] public IMDBMovieElements Movie { get; set; }
        [BsonRequired] public string? Title { get; set; }
        [BsonRequired] public DateTime? Updated { get; set; }
        //public string? Mark { get; set; }
        [BsonIgnoreIfNull] public DateTime? FirstCaptured { get; set; }
        [BsonIgnoreIfNull] public DateTime? MarkDate { get; set; }
        //[BsonIgnoreIfNull] public IMDBMovies_MarkWhy MarkWhy { get; set; }
        [BsonIgnoreIfNull] public string? NeverMore { get; set; }

        [BsonExtraElements] public BsonDocument _CatchAll { get; set; }
    }


}
