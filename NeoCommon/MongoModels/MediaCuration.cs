using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using NeoCommon.MongoModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCommon.MongoModels
{
    public static class MediaCuration_exts
    {
        public static IMongoCollection<MediaCuration> MediaCuration(this IMongoDatabase db)
        {
            return db.GetCollection<MediaCuration>("MediaCuration");
        }
    }
    public class MediaCuration_MediaInfo
    {
        [BsonIgnoreIfNull] public string count { get; set; }    // 24/58642
        [BsonIgnoreIfNull] public string count_of_stream_of_this_kind { get; set; }     // 24/58642
        [BsonIgnoreIfNull] public string kind_of_stream { get; set; }   // 24/58642
        [BsonIgnoreIfNull] public List<string> other_kind_of_stream { get; set; }       // 24/58642
        [BsonIgnoreIfNull] public string stream_identifier { get; set; }        // 24/58642
        [BsonIgnoreIfNull] public string track_type { get; set; }       // 24/58642
        [BsonIgnoreIfNull] public string commercial_name { get; set; }  // 22/58642
        [BsonIgnoreIfNull] public string format { get; set; }   // 22/58642
        [BsonIgnoreIfNull] public List<string> other_format { get; set; }       // 22/58642
        [BsonIgnoreIfNull] public string title { get; set; }    // 22/58642
        [BsonIgnoreIfNull] public string unique_id { get; set; }        // 22/58642
        [BsonIgnoreIfNull] public string codec_id { get; set; } // 20/58642
        [BsonIgnoreIfNull][BsonElement("default")] public string a_default { get; set; }  // 20/58642
        [BsonIgnoreIfNull] public string forced { get; set; }   // 20/58642
        [BsonIgnoreIfNull] public string language { get; set; } // 20/58642
        [BsonIgnoreIfNull] public List<string> other_default { get; set; }      // 20/58642
        [BsonIgnoreIfNull] public List<string> other_forced { get; set; }       // 20/58642
        [BsonIgnoreIfNull] public List<string> other_language { get; set; }     // 20/58642
        [BsonIgnoreIfNull] public List<string> other_track_id { get; set; }     // 20/58642
        [BsonIgnoreIfNull] public string streamorder { get; set; }      // 20/58642
        [BsonIgnoreIfNull] public string track_id { get; set; } // 20/58642
        [BsonIgnoreIfNull] public List<string> other_stream_identifier { get; set; }    // 18/58642
        [BsonIgnoreIfNull] public string codec_id_info { get; set; }    // 12/58642
        [BsonIgnoreIfNull] public string duration { get; set; } // 10/58642
        [BsonIgnoreIfNull] public string format_url { get; set; }       // 10/58642
        [BsonIgnoreIfNull] public string frame_rate { get; set; }       // 10/58642
        [BsonIgnoreIfNull] public List<string> other_duration { get; set; }     // 10/58642
        [BsonIgnoreIfNull] public List<string> other_frame_rate { get; set; }   // 10/58642
        [BsonIgnoreIfNull] public List<string> other_stream_size { get; set; }  // 10/58642
        [BsonIgnoreIfNull] public string proportion_of_this_stream { get; set; }        // 10/58642
        [BsonIgnoreIfNull] public string stream_size { get; set; }      // 10/58642
        [BsonIgnoreIfNull] public string bit_rate { get; set; } // 8/58642
        [BsonIgnoreIfNull] public string delay { get; set; }    // 8/58642
        [BsonIgnoreIfNull] public string delay__origin { get; set; }    // 8/58642
        [BsonIgnoreIfNull] public string format_info { get; set; }      // 8/58642
        [BsonIgnoreIfNull] public List<string> other_bit_rate { get; set; }     // 8/58642
        [BsonIgnoreIfNull] public List<string> other_delay { get; set; }        // 8/58642
        [BsonIgnoreIfNull] public List<string> other_delay__origin { get; set; }        // 8/58642
        [BsonIgnoreIfNull] public string acmod { get; set; }    // 6/58642
        [BsonIgnoreIfNull] public string bit_rate_mode { get; set; }    // 6/58642
        [BsonIgnoreIfNull] public string bsid { get; set; }     // 6/58642
        [BsonIgnoreIfNull] public string channel_layout { get; set; }   // 6/58642
        [BsonIgnoreIfNull] public string channel_positions { get; set; }        // 6/58642
        [BsonIgnoreIfNull] public string channel_s { get; set; }        // 6/58642
        [BsonIgnoreIfNull] public string compression_mode { get; set; } // 6/58642
        [BsonIgnoreIfNull] public string delay_relative_to_video { get; set; }  // 6/58642
        [BsonIgnoreIfNull] public string dialnorm { get; set; } // 6/58642
        [BsonIgnoreIfNull] public string dialnorm_average { get; set; } // 6/58642
        [BsonIgnoreIfNull] public string dialnorm_count { get; set; }   // 6/58642
        [BsonIgnoreIfNull] public string dialnorm_maximum { get; set; } // 6/58642
        [BsonIgnoreIfNull] public string dialnorm_minimum { get; set; } // 6/58642
        [BsonIgnoreIfNull] public string format_settings__endianness { get; set; }      // 6/58642
        [BsonIgnoreIfNull] public string lfeon { get; set; }    // 6/58642
        [BsonIgnoreIfNull] public List<string> other_bit_rate_mode { get; set; }        // 6/58642
        [BsonIgnoreIfNull] public List<string> other_channel_positions { get; set; }    // 6/58642
        [BsonIgnoreIfNull] public List<string> other_channel_s { get; set; }    // 6/58642
        [BsonIgnoreIfNull] public List<string> other_commercial_name { get; set; }      // 6/58642
        [BsonIgnoreIfNull] public List<string> other_compression_mode { get; set; }     // 6/58642
        [BsonIgnoreIfNull] public List<string> other_delay_relative_to_video { get; set; }      // 6/58642
        [BsonIgnoreIfNull] public List<string> other_dialnorm { get; set; }     // 6/58642
        [BsonIgnoreIfNull] public List<string> other_dialnorm_average { get; set; }     // 6/58642
        [BsonIgnoreIfNull] public List<string> other_dialnorm_maximum { get; set; }     // 6/58642
        [BsonIgnoreIfNull] public List<string> other_dialnorm_minimum { get; set; }     // 6/58642
        [BsonIgnoreIfNull] public List<string> other_sampling_rate { get; set; }        // 6/58642
        [BsonIgnoreIfNull] public List<string> other_service_kind { get; set; } // 6/58642
        [BsonIgnoreIfNull] public string samples_count { get; set; }    // 6/58642
        [BsonIgnoreIfNull] public string samples_per_frame { get; set; }        // 6/58642
        [BsonIgnoreIfNull] public string sampling_rate { get; set; }    // 6/58642
        [BsonIgnoreIfNull] public string service_kind { get; set; }     // 6/58642
        [BsonIgnoreIfNull] public string frame_count { get; set; }      // 4/58642
        [BsonIgnoreIfNull] public List<string> other_writing_library { get; set; }      // 4/58642
        [BsonIgnoreIfNull] public string writing_library { get; set; }  // 4/58642
        [BsonIgnoreIfNull][BsonElement("00_00_00000")] public string A_000000000 { get; set; } // 2/58642
        [BsonIgnoreIfNull][BsonElement("00_10_26376")] public string A_001026376 { get; set; } // 2/58642
        [BsonIgnoreIfNull][BsonElement("00_20_54628")] public string A_002054628 { get; set; } // 2/58642
        [BsonIgnoreIfNull][BsonElement("00_31_15290")] public string A_003115290 { get; set; } // 2/58642
        [BsonIgnoreIfNull][BsonElement("00_40_58498")] public string A_004058498 { get; set; } // 2/58642
        [BsonIgnoreIfNull][BsonElement("00_50_50714")] public string A_005050714 { get; set; } // 2/58642
        [BsonIgnoreIfNull][BsonElement("01_00_05435")] public string A_010005435 { get; set; } // 2/58642
        [BsonIgnoreIfNull][BsonElement("01_09_51813")] public string A_010951813 { get; set; } // 2/58642
        [BsonIgnoreIfNull][BsonElement("01_19_51245")] public string A_011951245 { get; set; } // 2/58642
        [BsonIgnoreIfNull][BsonElement("01_31_07295")] public string A_013107295 { get; set; } // 2/58642
        [BsonIgnoreIfNull][BsonElement("01_33_57131")] public string A_013357131 { get; set; } // 2/58642
        [BsonIgnoreIfNull] public string audio_codecs { get; set; }     // 2/58642
        [BsonIgnoreIfNull] public string audio_format_list { get; set; }        // 2/58642
        [BsonIgnoreIfNull] public string audio_format_withhint_list { get; set; }       // 2/58642
        [BsonIgnoreIfNull] public string audio_language_list { get; set; }      // 2/58642
        [BsonIgnoreIfNull] public string bit_depth { get; set; }        // 2/58642
        [BsonIgnoreIfNull] public string bits__pixel_frame { get; set; }        // 2/58642
        [BsonIgnoreIfNull] public string chapters_pos_begin { get; set; }       // 2/58642
        [BsonIgnoreIfNull] public string chapters_pos_end { get; set; } // 2/58642
        [BsonIgnoreIfNull] public string chroma_subsampling { get; set; }       // 2/58642
        [BsonIgnoreIfNull] public string codec_id_url { get; set; }     // 2/58642
        [BsonIgnoreIfNull] public string codecs_video { get; set; }     // 2/58642
        [BsonIgnoreIfNull] public string color_space { get; set; }      // 2/58642
        [BsonIgnoreIfNull] public string complete_name { get; set; }    // 2/58642
        [BsonIgnoreIfNull] public string compr_average { get; set; }    // 2/58642
        [BsonIgnoreIfNull] public string compr_count { get; set; }      // 2/58642
        [BsonIgnoreIfNull] public string compr_maximum { get; set; }    // 2/58642
        [BsonIgnoreIfNull] public string compr_minimum { get; set; }    // 2/58642
        [BsonIgnoreIfNull] public string count_of_audio_streams { get; set; }   // 2/58642
        [BsonIgnoreIfNull] public string count_of_menu_streams { get; set; }    // 2/58642
        [BsonIgnoreIfNull] public string count_of_text_streams { get; set; }    // 2/58642
        [BsonIgnoreIfNull] public string count_of_video_streams { get; set; }   // 2/58642
        [BsonIgnoreIfNull] public string display_aspect_ratio { get; set; }     // 2/58642
        [BsonIgnoreIfNull] public string dsurmod { get; set; }  // 2/58642
        [BsonIgnoreIfNull] public string dynrng_average { get; set; }   // 2/58642
        [BsonIgnoreIfNull] public string dynrng_count { get; set; }     // 2/58642
        [BsonIgnoreIfNull] public string dynrng_maximum { get; set; }   // 2/58642
        [BsonIgnoreIfNull] public string dynrng_minimum { get; set; }   // 2/58642
        [BsonIgnoreIfNull] public string encoded_date { get; set; }     // 2/58642
        [BsonIgnoreIfNull] public string encoded_library_name { get; set; }     // 2/58642
        [BsonIgnoreIfNull] public string encoded_library_version { get; set; }  // 2/58642
        [BsonIgnoreIfNull] public string encoding_settings { get; set; }        // 2/58642
        [BsonIgnoreIfNull] public string file_extension { get; set; }   // 2/58642
        [BsonIgnoreIfNull] public string file_last_modification_date { get; set; }      // 2/58642
        [BsonIgnoreIfNull] public string file_last_modification_date__local { get; set; }       // 2/58642
        [BsonIgnoreIfNull] public string file_name { get; set; }        // 2/58642
        [BsonIgnoreIfNull] public string file_name_extension { get; set; }      // 2/58642
        [BsonIgnoreIfNull] public string file_size { get; set; }        // 2/58642
        [BsonIgnoreIfNull] public string folder_name { get; set; }      // 2/58642
        [BsonIgnoreIfNull] public string format_extensions_usually_used { get; set; }   // 2/58642
        [BsonIgnoreIfNull] public string format_profile { get; set; }   // 2/58642
        [BsonIgnoreIfNull] public string format_settings { get; set; }  // 2/58642
        [BsonIgnoreIfNull] public string format_settings__cabac { get; set; }   // 2/58642
        [BsonIgnoreIfNull] public string format_settings__reference_frames { get; set; }        // 2/58642
        [BsonIgnoreIfNull] public string format_version { get; set; }   // 2/58642
        [BsonIgnoreIfNull] public string frame_rate_mode { get; set; }  // 2/58642
        [BsonIgnoreIfNull] public string framerate_den { get; set; }    // 2/58642
        [BsonIgnoreIfNull] public string framerate_num { get; set; }    // 2/58642
        [BsonIgnoreIfNull] public string height { get; set; }   // 2/58642
        [BsonIgnoreIfNull] public string internet_media_type { get; set; }      // 2/58642
        [BsonIgnoreIfNull] public string isstreamable { get; set; }     // 2/58642
        [BsonIgnoreIfNull] public string movie_name { get; set; }       // 2/58642
        [BsonIgnoreIfNull] public List<string> other_bit_depth { get; set; }    // 2/58642
        [BsonIgnoreIfNull] public List<string> other_chroma_subsampling { get; set; }   // 2/58642
        [BsonIgnoreIfNull] public List<string> other_compr_average { get; set; }        // 2/58642
        [BsonIgnoreIfNull] public List<string> other_compr_maximum { get; set; }        // 2/58642
        [BsonIgnoreIfNull] public List<string> other_compr_minimum { get; set; }        // 2/58642
        [BsonIgnoreIfNull] public List<string> other_display_aspect_ratio { get; set; } // 2/58642
        [BsonIgnoreIfNull] public List<string> other_dynrng_average { get; set; }       // 2/58642
        [BsonIgnoreIfNull] public List<string> other_dynrng_maximum { get; set; }       // 2/58642
        [BsonIgnoreIfNull] public List<string> other_dynrng_minimum { get; set; }       // 2/58642
        [BsonIgnoreIfNull] public List<string> other_file_size { get; set; }    // 2/58642
        [BsonIgnoreIfNull] public List<string> other_format_settings__cabac { get; set; }       // 2/58642
        [BsonIgnoreIfNull] public List<string> other_format_settings__reference_frames { get; set; }    // 2/58642
        [BsonIgnoreIfNull] public List<string> other_frame_rate_mode { get; set; }      // 2/58642
        [BsonIgnoreIfNull] public List<string> other_height { get; set; }       // 2/58642
        [BsonIgnoreIfNull] public List<string> other_overall_bit_rate { get; set; }     // 2/58642
        [BsonIgnoreIfNull] public List<string> other_scan_type { get; set; }    // 2/58642
        [BsonIgnoreIfNull] public List<string> other_unique_id { get; set; }    // 2/58642
        [BsonIgnoreIfNull] public List<string> other_width { get; set; }        // 2/58642
        [BsonIgnoreIfNull] public List<string> other_writing_application { get; set; }  // 2/58642
        [BsonIgnoreIfNull] public string overall_bit_rate { get; set; } // 2/58642
        [BsonIgnoreIfNull] public string pixel_aspect_ratio { get; set; }       // 2/58642
        [BsonIgnoreIfNull] public string sampled_height { get; set; }   // 2/58642
        [BsonIgnoreIfNull] public string sampled_width { get; set; }    // 2/58642
        [BsonIgnoreIfNull] public string scan_type { get; set; }        // 2/58642
        [BsonIgnoreIfNull] public string stored_height { get; set; }    // 2/58642
        [BsonIgnoreIfNull] public string text_codecs { get; set; }      // 2/58642
        [BsonIgnoreIfNull] public string text_format_list { get; set; } // 2/58642
        [BsonIgnoreIfNull] public string text_format_withhint_list { get; set; }        // 2/58642
        [BsonIgnoreIfNull] public string text_language_list { get; set; }       // 2/58642
        [BsonIgnoreIfNull] public string video_format_list { get; set; }        // 2/58642
        [BsonIgnoreIfNull] public string video_format_withhint_list { get; set; }       // 2/58642
        [BsonIgnoreIfNull] public string video_language_list { get; set; }      // 2/58642
        [BsonIgnoreIfNull] public string width { get; set; }    // 2/58642
        [BsonIgnoreIfNull] public string writing_application { get; set; }      // 2/58642
    }
    public class MediaCuration_bdf372d5_98f4_5a3e_b897_de0e13dcdbe4
    {
        [BsonIgnoreIfNull] public List<MediaCuration_MediaInfo> tracks { get; set; } // 2/58642
    }
    public class MediaCurationPartMap
    {
        [BsonIgnoreIfNull] public MediaCuration_bdf372d5_98f4_5a3e_b897_de0e13dcdbe4 map { get; set; }  // 1513/58642
    }
    public class MediaCuration_ba9adafc_7895_59b6_be50_7583f2d3cd99
    {
        [BsonIgnoreIfNull] public string name { get; set; }     // 2/58642
    }
    public class MediaCuration_2e915dde_b2fd_55b0_904b_a12626b395d2
    {
        [BsonIgnoreIfNull] public MediaCuration_ba9adafc_7895_59b6_be50_7583f2d3cd99 character { get; set; }    // 2/58642
        [BsonIgnoreIfNull] public string notes { get; set; }    // 2/58642
    }
    public class MediaCuration_Cast
    {
        [BsonIgnoreIfNull][BsonElement("current-role")] public MediaCuration_2e915dde_b2fd_55b0_904b_a12626b395d2 A_currentrole { get; set; }  // 2/58642
        [BsonIgnoreIfNull] public string id { get; set; }       // 2/58642
        [BsonIgnoreIfNull] public string name { get; set; }     // 2/58642
    }
    public class MediaCuration_Producer
    {
        [BsonIgnoreIfNull] public string id { get; set; }       // 2/58642
        [BsonIgnoreIfNull] public string name { get; set; }     // 2/58642
        [BsonIgnoreIfNull] public string notes { get; set; }    // 2/58642
    }
    public class MediaCuration_MediaWhy
    {
        [BsonIgnoreIfNull] public List<MediaCuration_Cast> cast { get; set; }   // 10776/58642
        [BsonIgnoreIfNull] public List<string> Keywords { get; set; }   // 4950/58642
        [BsonIgnoreIfNull] public List<string> TorKeywords { get; set; }        // 3232/58642
        [BsonIgnoreIfNull] public List<MediaCuration_Producer> producer { get; set; }       // 1742/58642
        [BsonIgnoreIfNull] public List<MediaCuration_Producer> director { get; set; }       // 1364/58642
        [BsonIgnoreIfNull] public string Rating { get; set; }   // 280/58642
        [BsonIgnoreIfNull] public List<MediaCuration_Producer> writer { get; set; } // 116/58642
    }
    public class MediaCuration
    {
        public string _id { get; set; }
        //[BsonRequired] public BsonDocument AllRatings { get; set; }     // 58642/58642
        //[BsonRequired] public BsonDocument Editions { get; set; }       // 58642/58642
        [BsonRequired] public List<string> Genres { get; set; } // 58642/58642
        [BsonRequired] public DateTime? InitialCapture { get; set; }    // 58642/58642
        [BsonRequired] public string Kind { get; set; } // 58642/58642
        [BsonRequired] public DateTime? LastSeen { get; set; }  // 58642/58642
        [BsonRequired] public string MaxResolution { get; set; }        // 58642/58642
        [BsonRequired] public string MaxResolutionDetail { get; set; }  // 58642/58642
        [BsonRequired] public UInt32? MediaFiles { get; set; }  // 58642/58642
        //[BsonRequired] public MediaCurationPartMap Parts { get; set; }    // 58642/58642
        [BsonRequired] public Byte[] Path { get; set; } // 58642/58642
        [BsonRequired] public string Rating { get; set; }       // 58642/58642
        [BsonRequired] public string Repo { get; set; } // 58642/58642
        [BsonRequired] public UInt64? Size { get; set; }        // 58642/58642
        [BsonRequired] public string Title { get; set; }        // 58642/58642
        [BsonRequired] public double? TotalRuntime { get; set; }        // 58642/58642
        [BsonRequired] public DateTime? Updated { get; set; }   // 58642/58642
        [BsonRequired] public UInt32? Year { get; set; }        // 58642/58642
        [BsonRequired] public string _ident { get; set; }       // 58642/58642
        [BsonIgnoreIfNull] public string TorLabel { get; set; } // 31744/58642
        [BsonIgnoreIfNull] public string TorrentId { get; set; }        // 31744/58642
        [BsonIgnoreIfNull] public List<string> IMDBs { get; set; }      // 31628/58642
        [BsonIgnoreIfNull] public bool? XXX { get; set; }       // 30950/58642
        //[BsonIgnoreIfNull] public MediaCuration_MediaWhy MarkWhy { get; set; }      // 17082/58642
        [BsonIgnoreIfNull] public bool? Adult { get; set; }     // 3846/58642
        [BsonIgnoreIfNull] public bool? Ok { get; set; }        // 600/58642
        [BsonIgnoreIfNull] public bool? MediaDelete { get; set; }       // 76/58642
        [BsonIgnoreIfNull] public string Putaway { get; set; }  // 76/58642
        [BsonIgnoreIfNull] public BsonDocument PutawayPool { get; set; }        // 76/58642
        [BsonIgnoreIfNull] public bool? Rework { get; set; }    // 76/58642

        [BsonExtraElements] public BsonDocument _CatchAll { get; set; }
    }


}
