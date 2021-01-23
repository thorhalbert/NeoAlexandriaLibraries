using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeoAssets.Mongo
{
    public class Assets 
    {
        public string _id { get; set; }
        [BsonRequired] public double ElasticUpdate { get; set; }   // 3997244/4000000
        [BsonRequired] public double Length { get; set; }  // 3996796/4000000
        [BsonRequired] public Assets_FileTypes FileType { get; set; }    // 3978688/4000000
        [BsonIgnoreIfNull] public BsonDocument Physical { get; set; }   // 3895070/4000000
        [BsonRequired] public string Universe { get; set; } // 3827652/4000000
        public Assets_MetaCaptures MetaCapture { get; set; }    // 3728430/4000000
        [BsonIgnoreIfNull] public Assets_Hashes Hashes { get; set; }  // 3536740/4000000
        [BsonIgnoreIfNull] public BsonDocument ContentVersions { get; set; }    // 3409560/4000000
        [BsonIgnoreIfNull] public BsonDocument Keywords { get; set; }   // 3409126/4000000
        [BsonIgnoreIfNull] public BsonDocument Matchwords { get; set; } // 3408904/4000000
        [BsonIgnore] public Assets_Baked Baked { get; set; }   // 3307780/4000000
        [BsonIgnoreIfNull] public string Title { get; set; }    // 3087518/4000000
        [BsonIgnoreIfNull] public bool? MetaFile { get; set; }  // 2783510/4000000
        [BsonIgnoreIfNull] public string Search { get; set; }   // 2689662/4000000
        [BsonIgnoreIfNull] public string ContentVerUUID { get; set; }   // 1806036/4000000
        [BsonIgnoreIfNull] public List<Assets_Archives> Archive { get; set; }   // 1133648/4000000
        [BsonIgnoreIfNull] public double? LastUpdate { get; set; }  // 1044768/4000000
        [BsonIgnoreIfNull] public Assets_Cuovers Cover { get; set; }   // 954744/4000000
        [BsonIgnoreIfNull] public string ArchiveKey { get; set; }   // 939014/4000000
        [BsonIgnoreIfNull] public UInt32? Pages { get; set; }   // 459814/4000000
        [BsonIgnoreIfNull] public List<string> TorrentSource { get; set; }  // 169014/4000000
        [BsonIgnoreIfNull] public Assets_CoverUnavailable CoverUnavailable { get; set; }    // 152470/4000000
        [BsonIgnoreIfNull] public string Processed { get; set; }    // 134632/4000000
        [BsonIgnoreIfNull] public Assets_Containers Container { get; set; }   // 126128/4000000
        [BsonIgnoreIfNull] public List<string> ISBNs { get; set; }  // 110436/4000000
        [BsonIgnoreIfNull] public string OL_Edition { get; set; }   // 110436/4000000
        [BsonIgnoreIfNull] public List<string> Owners { get; set; } // 80582/4000000
        [BsonIgnoreIfNull] public List<string> InStack { get; set; }    // 69636/4000000
        [BsonIgnoreIfNull] public Assets_Codes Codes { get; set; }   // 56492/4000000
        [BsonIgnoreIfNull] public string Language { get; set; } // 56214/4000000
        [BsonIgnoreIfNull] public string OL_Title { get; set; } // 48712/4000000
        [BsonIgnoreIfNull] public List<string> OL_Publishers { get; set; }  // 48506/4000000
        [BsonIgnoreIfNull] public DateTime? OL_Date { get; set; }   // 48400/4000000
        [BsonIgnoreIfNull] public List<string> OL_Works { get; set; }   // 45780/4000000
        [BsonIgnoreIfNull] public List<string> OL_Language { get; set; }    // 45014/4000000
        [BsonIgnoreIfNull] public List<string> OL_Authors { get; set; } // 41004/4000000
        [BsonIgnoreIfNull] public Assets_OLIdentifiers OL_Identifiers { get; set; }  // 40632/4000000
        [BsonIgnoreIfNull] public DateTime? TimeAdded { get; set; } // 36614/4000000
        [BsonIgnoreIfNull] public DateTime? TimeLastModified { get; set; }  // 36614/4000000
        [BsonIgnoreIfNull] public UInt32? Filesize { get; set; }    // 36606/4000000
        [BsonIgnoreIfNull] public bool? Matched { get; set; }   // 36604/4000000
        [BsonIgnoreIfNull] public string VolumeInfo { get; set; }   // 36382/4000000
        [BsonIgnoreIfNull] public Assets_Covers2 Covers { get; set; }  // 35646/4000000
        [BsonIgnoreIfNull] public UInt32? OL_Pages { get; set; }    // 35200/4000000
        [BsonIgnoreIfNull] public List<string> OL_Subjects { get; set; }    // 34878/4000000
        [BsonIgnoreIfNull] public List<string> Author { get; set; } // 34398/4000000
        [BsonIgnoreIfNull] public UInt32? PublishYear { get; set; } // 33034/4000000
        [BsonIgnoreIfNull] public string Publisher { get; set; }    // 29944/4000000
        [BsonIgnoreIfNull] public UInt32? OrgPages { get; set; }    // 28872/4000000
        [BsonIgnoreIfNull] public string OL_By { get; set; }    // 25984/4000000
        [BsonIgnoreIfNull] public string Issue { get; set; }    // 24466/4000000
        [BsonIgnoreIfNull] public UInt32? PagesInFile { get; set; } // 24386/4000000
        [BsonIgnoreIfNull] public string OL_Format { get; set; }    // 23722/4000000
        [BsonIgnoreIfNull] public List<string> OL_LCC { get; set; } // 22496/4000000
        [BsonIgnoreIfNull] public List<string> OL_LCCN { get; set; }    // 20732/4000000
        [BsonIgnoreIfNull] [BsonElement("IA-Collections")] public List<string> IA_Collections { get; set; }    // 20386/4000000
        [BsonIgnoreIfNull] [BsonElement("IA-File")] public Assets_IAFiles IAFile { get; set; }   // 20386/4000000
        [BsonIgnoreIfNull] [BsonElement("IA-Misc")] public Assets_IAMisc IAMisc { get; set; }   // 20386/4000000
        [BsonIgnoreIfNull] public string Source { get; set; }   // 20386/4000000
        [BsonIgnoreIfNull] public string MediaType { get; set; }    // 19220/4000000
        [BsonIgnoreIfNull] public string Description { get; set; }  // 19194/4000000
        [BsonIgnoreIfNull] public List<string> OL_Dewey { get; set; }   // 19172/4000000
        [BsonIgnoreIfNull] public string Topic { get; set; }    // 17186/4000000
        [BsonIgnoreIfNull] public Assets_Torrents Torrent { get; set; } // 14276/4000000
        [BsonIgnoreIfNull] public string Edition { get; set; }  // 13858/4000000
        [BsonIgnoreIfNull] public string Searchable { get; set; }   // 13638/4000000
        [BsonIgnoreIfNull] public string Library { get; set; }  // 9670/4000000
        [BsonIgnoreIfNull] public string Scanned { get; set; }  // 9300/4000000
        [BsonIgnoreIfNull] public string Series { get; set; }   // 8702/4000000
        [BsonIgnoreIfNull] public string Tags { get; set; } // 8566/4000000
        [BsonIgnoreIfNull] public string Subtitle { get; set; } // 8046/4000000
        [BsonIgnoreIfNull] public List<string> OL_Genre { get; set; }   // 7860/4000000
        [BsonIgnoreIfNull] public string IdentifierWODash { get; set; } // 5084/4000000
        [BsonIgnoreIfNull] public string PublishCity { get; set; }  // 4782/4000000
        [BsonIgnoreIfNull] public string Visible { get; set; }  // 4770/4000000
        [BsonIgnoreIfNull] public string Bookmarked { get; set; }   // 4036/4000000
        [BsonIgnoreIfNull] public string Paginated { get; set; }    // 2978/4000000
        [BsonIgnoreIfNull] public string Commentary { get; set; }   // 2850/4000000
        [BsonIgnoreIfNull] public string Year { get; set; } // 2296/4000000
        [BsonIgnoreIfNull] public string OL_Subtitle { get; set; }  // 2036/4000000
        [BsonIgnoreIfNull] public UInt32? DPI { get; set; } // 1404/4000000
        [BsonIgnoreIfNull] public UInt32? Local { get; set; }   // 1182/4000000
        [BsonIgnoreIfNull] public string Generic { get; set; }  // 894/4000000
        [BsonIgnoreIfNull] public string Volume { get; set; }   // 840/4000000
        [BsonIgnoreIfNull] public string Color { get; set; }    // 658/4000000
        [BsonIgnoreIfNull] public bool? Tagger { get; set; }    // 574/4000000
        [BsonIgnoreIfNull] public string Periodical { get; set; }   // 410/4000000
        [BsonIgnoreIfNull] public string Cleaned { get; set; }  // 346/4000000
        [BsonIgnoreIfNull] public string Emulator { get; set; } // 202/4000000
        [BsonIgnoreIfNull] public string Emulator_ext { get; set; } // 202/4000000
        [BsonIgnoreIfNull] public string Orientation { get; set; }  // 120/4000000
        [BsonIgnoreIfNull] public string Notes { get; set; }    // 34/4000000
        [BsonIgnoreIfNull] public List<string> IACollections { get; set; }  // 20/4000000
        [BsonIgnoreIfNull] public bool? Handoff { get; set; }   // 16/4000000
        [BsonIgnore] public Assets_Attempts Attempts { get; set; }    // 2/4000000
        [BsonIgnore] public List<string> Collections { get; set; }    // 2/4000000
        [BsonIgnore] public List<string> CollectionsFlat { get; set; }    // 2/4000000
        [BsonIgnoreIfNull] public DateTime? Expire { get; set; }    // 2/4000000
        [BsonIgnore] public string Isbn1Version { get; set; } // 2/4000000
        [BsonIgnore] public DateTime? MatchRun { get; set; }  // 2/4000000
        [BsonIgnore] public Assets_Reprocess Reprocess { get; set; }   // 2/4000000
        [BsonIgnore] public string SearchType { get; set; }   // 2/4000000
        [BsonIgnoreIfNull] public UInt32? Version { get; set; } // 2/4000000

        [BsonExtraElements] public BsonDocument _CatchAll { get; set; }
    }

    public class Assets_Media_Files
    {
        [BsonIgnoreIfNull] public List<Assets_Media_Tracks> track { get; set; } // 2/4000000
    }
    public class Assets_Opf_Creators
    {
        [BsonIgnoreIfNull] public string role { get; set; } // 4/4000000
        [BsonIgnoreIfNull] public string value { get; set; }    // 4/4000000
        [BsonIgnoreIfNull] [BsonElement("file-as")] public string file_as { get; set; }    // 2/4000000
    }
    public class Assets_Reprocess
    {
        [BsonIgnoreIfNull] public DateTime? Expire { get; set; }    // 2/4000000
        [BsonIgnoreIfNull] public UInt32? Version { get; set; } // 2/4000000
    }
    public class Assets_Archives
    {
        [BsonIgnoreIfNull] public string Archive { get; set; }  // 2/4000000
        [BsonIgnoreIfNull] public string File { get; set; } // 2/4000000
        [BsonIgnoreIfNull] public string Volume { get; set; }   // 2/4000000
    }
    public class Assets_Codes
    {
        [BsonIgnoreIfNull] public UInt32? LibGen { get; set; }  // 36614/4000000
        [BsonIgnoreIfNull] public string LibGenURI { get; set; }    // 35158/4000000
        [BsonIgnoreIfNull] public List<string> ISBN { get; set; }   // 29432/4000000
        [BsonIgnoreIfNull] [BsonElement("IA-identifier")] public string IA_identifier { get; set; }    // 19878/4000000
        [BsonIgnoreIfNull] [BsonElement("identifier-ark")] public string identifier_ark { get; set; }  // 18364/4000000
        [BsonIgnoreIfNull] [BsonElement("identifier-access")] public string identifier_access { get; set; }    // 18020/4000000
        [BsonIgnoreIfNull] public string OL { get; set; }   // 13816/4000000
        [BsonIgnoreIfNull] public string DD { get; set; }   // 7752/4000000
        [BsonIgnoreIfNull] public string LCC { get; set; }  // 7268/4000000
        [BsonIgnoreIfNull] public string GoogleBookId { get; set; } // 2328/4000000
        [BsonIgnoreIfNull] public string DOI { get; set; }  // 1686/4000000
        [BsonIgnoreIfNull] public string ASIN { get; set; } // 850/4000000
        [BsonIgnoreIfNull] public string ISSN { get; set; } // 418/4000000
        [BsonIgnoreIfNull] public string UDC { get; set; }  // 20/4000000
        [BsonIgnoreIfNull] public string LBC { get; set; }  // 8/4000000
    }
    public class Assets_Meta_MediaInfo_FilesTracks
    {
        [BsonIgnoreIfNull] public string Complete_name { get; set; }    // 8272/4000000
        [BsonIgnoreIfNull] public string File_size { get; set; }    // 8272/4000000
        [BsonIgnoreIfNull] public string Format { get; set; }   // 394/4000000
        [BsonIgnoreIfNull] public string Format_profile { get; set; }   // 8/4000000
        [BsonIgnoreIfNull] public string Codec_ID { get; set; } // 4/4000000
        [BsonIgnoreIfNull] public string Format_Info { get; set; }  // 4/4000000
        [BsonIgnoreIfNull] public string Encoded_date { get; set; } // 2/4000000
        [BsonIgnoreIfNull] public string Tagged_date { get; set; }  // 2/4000000
    }
    public class Asset_Opf_Body_Guides
    {
        [BsonIgnoreIfNull] public BsonDocument reference { get; set; }  // 4/4000000
    }

    public class Assets_13e41632_147e_5370_bece_903cd5e7f459
    {
        [BsonIgnoreIfNull] public Assets_MediaInfo Mediainfo { get; set; }   // 2/4000000
    }
    public class Assets_Opf_Body
    {
        [BsonIgnoreIfNull] public Assets_Opf_Body_Packages package { get; set; } // 12772/4000000
    }
    public class Assets_Cuovers
    {
        [BsonIgnoreIfNull] public DateTime? NoCover { get; set; }   // 8/4000000
    }
    public class Assets_Meta_MediaInfo
    {
        [BsonIgnoreIfNull] public DateTime? CaptureDate { get; set; }   // 129712/4000000
        [BsonIgnoreIfNull] public Assets_Meta_MediaInfo_Files File { get; set; }    // 129712/4000000
    }
    public class Assets_Opf_Body_Manifests
    {
        [BsonIgnoreIfNull] public BsonDocument item { get; set; }   // 6/4000000
    }
    public class Assets_Attempts
    {
        [BsonIgnoreIfNull] public DateTime? Cover { get; set; } // 2/4000000
    }
    public class Assets_Media_Tracks
    {
        [BsonIgnoreIfNull] public string Codec { get; set; }    // 6/4000000
        [BsonIgnoreIfNull] public string Commercial_name { get; set; }  // 6/4000000
        [BsonIgnoreIfNull] public string Count { get; set; }    // 6/4000000
        [BsonIgnoreIfNull] public string Count_of_stream_of_this_kind { get; set; } // 6/4000000
        [BsonIgnoreIfNull] public string Duration { get; set; } // 6/4000000
        [BsonIgnoreIfNull] public string Format { get; set; }   // 6/4000000
        [BsonIgnoreIfNull] public string Kind_of_stream { get; set; }   // 6/4000000
        [BsonIgnoreIfNull] public string Proportion_of_this_stream { get; set; }    // 6/4000000
        [BsonIgnoreIfNull] public string Stream_identifier { get; set; }    // 6/4000000
        [BsonIgnoreIfNull] public string Stream_size { get; set; }  // 6/4000000
        [BsonIgnoreIfNull] public string Bit_rate { get; set; } // 4/4000000
        [BsonIgnoreIfNull] public string Codec_Family { get; set; } // 4/4000000
        [BsonIgnoreIfNull] public string Codec_ID { get; set; } // 4/4000000
        [BsonIgnoreIfNull] public string Codec_Url { get; set; }    // 4/4000000
        [BsonIgnoreIfNull] public string Delay { get; set; }    // 4/4000000
        [BsonIgnoreIfNull] public string Delay__origin { get; set; }    // 4/4000000
        [BsonIgnoreIfNull] public string Format_Info { get; set; }  // 4/4000000
        [BsonIgnoreIfNull] public string Format_Url { get; set; }   // 4/4000000
        [BsonIgnoreIfNull] public string Format_profile { get; set; }   // 4/4000000
        [BsonIgnoreIfNull] public string Internet_media_type { get; set; }  // 4/4000000
        [BsonIgnoreIfNull] public string Resolution { get; set; }   // 4/4000000
        [BsonIgnoreIfNull] public string Audio_Format_List { get; set; }    // 2/4000000
        [BsonIgnoreIfNull] public string Audio_Format_WithHint_List { get; set; }   // 2/4000000
        [BsonIgnoreIfNull] public string Audio_codecs { get; set; } // 2/4000000
        [BsonIgnoreIfNull] public string Bit_depth { get; set; }    // 2/4000000
        [BsonIgnoreIfNull] public string Bits__Pixel_Frame_ { get; set; }   // 2/4000000
        [BsonIgnoreIfNull] public string Channel_positions { get; set; }    // 2/4000000
        [BsonIgnoreIfNull] public string Channel_s_ { get; set; }   // 2/4000000
        [BsonIgnoreIfNull] public string Chroma_subsampling { get; set; }   // 2/4000000
        [BsonIgnoreIfNull] public string Codec_Extensions_usually_used { get; set; }    // 2/4000000
        [BsonIgnoreIfNull] public string Codec_Info { get; set; }   // 2/4000000
        [BsonIgnoreIfNull] public string Codec_Settings_RefFrames { get; set; } // 2/4000000
        [BsonIgnoreIfNull] public string Codec_profile { get; set; }    // 2/4000000
        [BsonIgnoreIfNull] public string Codec_settings { get; set; }   // 2/4000000
        [BsonIgnoreIfNull] public string Codec_settings__CABAC { get; set; }    // 2/4000000
        [BsonIgnoreIfNull] public string Codecs_Video { get; set; } // 2/4000000
        [BsonIgnoreIfNull] public string Color_space { get; set; }  // 2/4000000
        [BsonIgnoreIfNull] public string Colorimetry { get; set; }  // 2/4000000
        [BsonIgnoreIfNull] public string Complete_name { get; set; }    // 2/4000000
        [BsonIgnoreIfNull] public string Compression_mode { get; set; } // 2/4000000
        [BsonIgnoreIfNull] public string Count_of_audio_streams { get; set; }   // 2/4000000
        [BsonIgnoreIfNull] public string Count_of_video_streams { get; set; }   // 2/4000000
        [BsonIgnoreIfNull] public string Delay_relative_to_video { get; set; }  // 2/4000000
        [BsonIgnoreIfNull] public string Display_aspect_ratio { get; set; } // 2/4000000
        [BsonIgnoreIfNull] public string Encoding_settings { get; set; }    // 2/4000000
        [BsonIgnoreIfNull] public string File_extension { get; set; }   // 2/4000000
        [BsonIgnoreIfNull] public string File_last_modification_date { get; set; }  // 2/4000000
        [BsonIgnoreIfNull] public string File_last_modification_date__local_ { get; set; }  // 2/4000000
        [BsonIgnoreIfNull] public string File_name { get; set; }    // 2/4000000
        [BsonIgnoreIfNull] public string File_size { get; set; }    // 2/4000000
        [BsonIgnoreIfNull] public string Folder_name { get; set; }  // 2/4000000
        [BsonIgnoreIfNull] public string Format_Extensions_usually_used { get; set; }   // 2/4000000
        [BsonIgnoreIfNull] public string Format_settings { get; set; }  // 2/4000000
        [BsonIgnoreIfNull] public string Format_settings__CABAC { get; set; }   // 2/4000000
        [BsonIgnoreIfNull] public string Format_settings__ReFrames { get; set; }    // 2/4000000
        [BsonIgnoreIfNull] public string FrameRate_Mode_Original { get; set; }  // 2/4000000
        [BsonIgnoreIfNull] public string Frame_count { get; set; }  // 2/4000000
        [BsonIgnoreIfNull] public string Frame_rate { get; set; }   // 2/4000000
        [BsonIgnoreIfNull] public string Frame_rate_mode { get; set; }  // 2/4000000
        [BsonIgnoreIfNull] public string Height { get; set; }   // 2/4000000
        [BsonIgnoreIfNull] public string Interlacement { get; set; }    // 2/4000000
        [BsonIgnoreIfNull] public string Overall_bit_rate { get; set; } // 2/4000000
        [BsonIgnoreIfNull] public string Pixel_aspect_ratio { get; set; }   // 2/4000000
        [BsonIgnoreIfNull] public string Samples_count { get; set; }    // 2/4000000
        [BsonIgnoreIfNull] public string Sampling_rate { get; set; }    // 2/4000000
        [BsonIgnoreIfNull] public string Scan_type { get; set; }    // 2/4000000
        [BsonIgnoreIfNull] public string Tagged_date { get; set; }  // 2/4000000
        [BsonIgnoreIfNull] public string Tagging_application { get; set; }  // 2/4000000
        [BsonIgnoreIfNull] public string Video_Format_List { get; set; }    // 2/4000000
        [BsonIgnoreIfNull] public string Video_Format_WithHint_List { get; set; }   // 2/4000000
        [BsonIgnoreIfNull] public string Video_delay { get; set; }  // 2/4000000
        [BsonIgnoreIfNull] public string Width { get; set; }    // 2/4000000
        [BsonIgnoreIfNull] public string Writing_library { get; set; }  // 2/4000000
        [BsonIgnoreIfNull] public string Writing_library_Name { get; set; } // 2/4000000
        [BsonIgnoreIfNull] public string Writing_library_Version { get; set; }  // 2/4000000
        [BsonIgnoreIfNull] public string totalframes { get; set; }  // 2/4000000
    }
    public class Assets_Hashes
    {
        [BsonIgnoreIfNull] public string MD5 { get; set; }  // 3536740/4000000
        [BsonIgnoreIfNull] public string SHA256 { get; set; }   // 3536740/4000000
        [BsonIgnoreIfNull] public string TIGER { get; set; }    // 3536740/4000000
        [BsonIgnoreIfNull] public string WHIRLPOOL { get; set; }    // 3536740/4000000
        [BsonIgnoreIfNull] public string SHA1 { get; set; } // 1229364/4000000
    }
    public class Assets_IAMisc
    {
        [BsonIgnoreIfNull] public string addeddate { get; set; }    // 20386/4000000
        [BsonIgnoreIfNull] public string publicdate { get; set; }   // 20386/4000000
        [BsonIgnoreIfNull] public string uploader { get; set; } // 20386/4000000
        [BsonIgnoreIfNull] public string ocr { get; set; }  // 19952/4000000
        [BsonIgnoreIfNull] public string backup_location { get; set; }  // 19030/4000000
        [BsonIgnoreIfNull] public string repub_state { get; set; }  // 19014/4000000
        [BsonIgnoreIfNull] public string ppi { get; set; }  // 18788/4000000
        [BsonIgnoreIfNull] public string date { get; set; } // 9116/4000000
        [BsonIgnoreIfNull] public string scanner { get; set; }  // 8622/4000000
        [BsonIgnoreIfNull] [BsonElement("bitsavers-filename")] public string BitsaversFilename { get; set; }  // 7490/4000000
        [BsonIgnoreIfNull] public string imagecount { get; set; }   // 5718/4000000
        [BsonIgnoreIfNull] public string curation { get; set; } // 2206/4000000
        [BsonIgnoreIfNull] public string foldoutcount { get; set; } // 1950/4000000
        [BsonIgnoreIfNull] public string creator { get; set; }  // 1334/4000000
        [BsonIgnoreIfNull] public string mediatype { get; set; }    // 1166/4000000
        [BsonIgnoreIfNull] public string coverleaf { get; set; }    // 828/4000000
        [BsonIgnoreIfNull] public string updatedate { get; set; }   // 494/4000000
        [BsonIgnoreIfNull] public string updater { get; set; }  // 494/4000000
        [BsonIgnoreIfNull] public string publisher { get; set; }    // 408/4000000
        [BsonIgnoreIfNull] public string coverage { get; set; } // 384/4000000
        [BsonIgnoreIfNull] public string licenseurl { get; set; }   // 218/4000000
        [BsonIgnoreIfNull] public string contributor { get; set; }  // 182/4000000
        [BsonIgnoreIfNull] public string filesxml { get; set; } // 170/4000000
        [BsonIgnoreIfNull] public string isbn { get; set; } // 104/4000000
        [BsonIgnoreIfNull] public string olsearch { get; set; } // 90/4000000
        [BsonIgnoreIfNull] public string issn { get; set; } // 66/4000000
        [BsonIgnoreIfNull] public string issue { get; set; }    // 62/4000000
        [BsonIgnoreIfNull] public string openlibrary_edition { get; set; }  // 62/4000000
        [BsonIgnoreIfNull] public string openlibrary_work { get; set; } // 62/4000000
        [BsonIgnoreIfNull] public string openlibrary { get; set; }  // 54/4000000
        [BsonIgnoreIfNull] public string rights { get; set; }   // 54/4000000
        [BsonIgnoreIfNull] public string lccn { get; set; } // 48/4000000
        [BsonIgnoreIfNull] public string volume { get; set; }   // 48/4000000
        [BsonIgnoreIfNull] [BsonElement("fixed-ppi")] public string FixedPPI { get; set; }    // 44/4000000
        [BsonIgnoreIfNull] [BsonElement("ppi-direct")] public string PPIDirect { get; set; }  // 42/4000000
        [BsonIgnoreIfNull] public string crack_number { get; set; } // 40/4000000
        [BsonIgnoreIfNull] public string noarchivetorrent { get; set; } // 40/4000000
        [BsonIgnoreIfNull] public string emulator_start { get; set; }   // 28/4000000
        [BsonIgnoreIfNull] public string sponsor { get; set; }  // 18/4000000
        [BsonIgnoreIfNull] [BsonElement("operator")] public string Operator { get; set; } // 14/4000000
        [BsonIgnoreIfNull] public string rutrackerid { get; set; }  // 14/4000000
        [BsonIgnoreIfNull] public string scandate { get; set; } // 14/4000000
        [BsonIgnoreIfNull] public string scanningcenter { get; set; }   // 14/4000000
        [BsonIgnoreIfNull] public string camera { get; set; }   // 12/4000000
        [BsonIgnoreIfNull] public string manual_item { get; set; }  // 10/4000000
        [BsonIgnoreIfNull] public string notes { get; set; }    // 10/4000000
        [BsonIgnoreIfNull] [BsonElement("oclc-id")] public string OCLCId { get; set; }    // 10/4000000
        [BsonIgnoreIfNull] public string scanfee { get; set; }  // 10/4000000
        [BsonIgnoreIfNull] public string sponsordate { get; set; }  // 10/4000000
        [BsonIgnoreIfNull] public string betterpdf { get; set; }    // 8/4000000
        [BsonIgnoreIfNull] public string invoice { get; set; }  // 8/4000000
        [BsonIgnoreIfNull] public string keywords { get; set; } // 8/4000000
        [BsonIgnoreIfNull] public string number { get; set; }   // 8/4000000
        [BsonIgnoreIfNull] [BsonElement("page-progression")] public string PageProgression { get; set; }  // 8/4000000
        [BsonIgnoreIfNull] public string repub_seconds { get; set; }    // 8/4000000
        [BsonIgnoreIfNull] public string republisher { get; set; }  // 8/4000000
        [BsonIgnoreIfNull] public string shiptracking { get; set; } // 8/4000000
        [BsonIgnoreIfNull] public string subjects { get; set; } // 8/4000000
        [BsonIgnoreIfNull] public string uploaded_by { get; set; }  // 8/4000000
        [BsonIgnoreIfNull] [BsonElement("possible-copyright-status")] public string PossibleCopyrightStatus { get; set; } // 6/4000000
        [BsonIgnoreIfNull] public string city { get; set; } // 4/4000000
        [BsonIgnoreIfNull] public string colleciton { get; set; }   // 4/4000000
        [BsonIgnoreIfNull] [BsonElement("copyright-evidence")] public string CopyrightEvidence { get; set; }  // 4/4000000
        [BsonIgnoreIfNull] [BsonElement("copyright-evidence-date")] public string CopyrightEvidenceDate { get; set; } // 4/4000000
        [BsonIgnoreIfNull] [BsonElement("copyright-evidence-operator")] public string CopyrightEvidencePperator { get; set; } // 4/4000000
        [BsonIgnoreIfNull] [BsonElement("copyright-region")] public string CopyrightRegion { get; set; }  // 4/4000000
        [BsonIgnoreIfNull] public string lcamid { get; set; }   // 4/4000000
        [BsonIgnoreIfNull] public string mobygames { get; set; }    // 4/4000000
        [BsonIgnoreIfNull] public string rcamid { get; set; }   // 4/4000000
        [BsonIgnoreIfNull] public string source { get; set; }   // 4/4000000
        [BsonIgnoreIfNull] public string wikipedia { get; set; }    // 4/4000000
        [BsonIgnoreIfNull] public string author { get; set; }   // 2/4000000
        [BsonIgnoreIfNull] public string call_number { get; set; }  // 2/4000000
        [BsonIgnoreIfNull] public string closed_captioning { get; set; }    // 2/4000000
        [BsonIgnoreIfNull] public string color { get; set; }    // 2/4000000
        [BsonIgnoreIfNull] public string edition { get; set; }  // 2/4000000
        [BsonIgnoreIfNull] [BsonElement("external-identifier")] public string ExternalIdentifier { get; set; }    // 2/4000000
        [BsonIgnoreIfNull] public string ia_orig__runtime { get; set; } // 2/4000000
        [BsonIgnoreIfNull] public string lang { get; set; } // 2/4000000
        [BsonIgnoreIfNull] public string manual { get; set; }   // 2/4000000
        [BsonIgnoreIfNull] public BsonDocument missingpages { get; set; }   // 2/4000000
        [BsonIgnoreIfNull] [BsonElement("no-preview")] public string NoPreview { get; set; }  // 2/4000000
        [BsonIgnoreIfNull] public string original_url { get; set; } // 2/4000000
        [BsonIgnoreIfNull] public string page { get; set; } // 2/4000000
        [BsonIgnoreIfNull] [BsonElement("pdfconvert-timeout")] public string PDFConvertTimeout { get; set; }  // 2/4000000
        [BsonIgnoreIfNull] [BsonElement("previous-collection")] public string PreviousCollection { get; set; }    // 2/4000000
        [BsonIgnoreIfNull] [BsonElement("related-external-id")] public string RelatedExternalId { get; set; } // 2/4000000
        [BsonIgnoreIfNull] public string runtime { get; set; }  // 2/4000000
        [BsonIgnoreIfNull] public string scanfactors { get; set; }  // 2/4000000
        [BsonIgnoreIfNull] public string sound { get; set; }    // 2/4000000
        [BsonIgnoreIfNull] public string translator { get; set; }   // 2/4000000
    }
    public class Assets_CoverUnavailable
    {
        [BsonIgnoreIfNull] public DateTime? Attempt { get; set; }   // 152470/4000000
        [BsonIgnoreIfNull] public string Version { get; set; }  // 152470/4000000
    }
    public class Assets_Opf_Body_MetaData
    {
        [BsonIgnoreIfNull] public string contributor { get; set; }  // 12772/4000000
        [BsonIgnoreIfNull] public string creator { get; set; }  // 12772/4000000
        [BsonIgnoreIfNull] public string identifier { get; set; }   // 12772/4000000
        [BsonIgnoreIfNull] public string language { get; set; } // 12772/4000000
        [BsonIgnoreIfNull] public string title { get; set; }    // 12772/4000000
        [BsonIgnoreIfNull] public string subject { get; set; }  // 42/4000000
        [BsonIgnoreIfNull] public string publisher { get; set; }    // 18/4000000
        [BsonIgnoreIfNull] public string date { get; set; } // 8/4000000
        [BsonIgnoreIfNull] public string description { get; set; }  // 6/4000000
        [BsonIgnoreIfNull] public BsonDocument meta { get; set; }   // 4/4000000
        [BsonIgnoreIfNull] public string rights { get; set; }   // 4/4000000
    }
    public class Assets_Covers2
    {
        [BsonIgnoreIfNull] public string LibGen { get; set; }   // 35646/4000000
        [BsonIgnoreIfNull] public Assets_Covers2_Front Front { get; set; }   // 29600/4000000
    }

    public class Assets_MetaCaptures
    {
        public Assets_Meta_Opf Opf { get; set; }    // 3728420/4000000
        [BsonIgnoreIfNull] public Assets_Meta_PdfInfo PdfInfo { get; set; } // 430794/4000000
        [BsonIgnoreIfNull] public Assets_Meta_MediaInfo MediaInfo { get; set; }   // 129712/4000000

    }
    public class Assets_Torrents
    {
        [BsonIgnoreIfNull] public BsonDocument Candidates { get; set; } // 14276/4000000
        [BsonIgnoreIfNull] public DateTime? LastScan { get; set; }  // 14276/4000000
    }
    public class Assets_IAFiles
    {
        [BsonIgnoreIfNull] public string crc32 { get; set; }    // 20386/4000000
        [BsonIgnoreIfNull] public string format { get; set; }   // 20386/4000000
        [BsonIgnoreIfNull] public string md5 { get; set; }  // 20386/4000000
        [BsonIgnoreIfNull] public string mtime { get; set; }    // 20386/4000000
        [BsonIgnoreIfNull] public string name { get; set; } // 20386/4000000
        [BsonIgnoreIfNull] public string sha1 { get; set; } // 20386/4000000
        [BsonIgnoreIfNull] public string size { get; set; } // 20386/4000000
        [BsonIgnoreIfNull] public string source { get; set; }   // 20386/4000000
        [BsonIgnoreIfNull] public string original { get; set; } // 216/4000000
        [BsonIgnoreIfNull] public string title { get; set; }    // 206/4000000
        [BsonIgnoreIfNull] public string height { get; set; }   // 24/4000000
        [BsonIgnoreIfNull] public string width { get; set; }    // 24/4000000
        [BsonIgnoreIfNull] public string length { get; set; }   // 22/4000000
        [BsonIgnoreIfNull] public string rotation { get; set; } // 6/4000000
        [BsonIgnoreIfNull] [BsonElement("private")] public string isPrivate { get; set; }    // 4/4000000
    }
    public class Assets_Opf_Identifiers
    {
        [BsonIgnoreIfNull] public string scheme { get; set; }   // 4/4000000
        [BsonIgnoreIfNull] public string value { get; set; }    // 4/4000000
        [BsonIgnoreIfNull] public string id { get; set; }   // 2/4000000
    }
    public class Assets_Meta_Opf
    {
        [BsonIgnoreIfNull] public DateTime? CaptureDate { get; set; }   // 1816378/4000000
        [BsonIgnoreIfNull] public Assets_Opf_Contributors contributor { get; set; } // 1803606/4000000
        [BsonIgnoreIfNull] public List<Assets_Opf_Creators> creator { get; set; }   // 1803606/4000000
        [BsonIgnoreIfNull] public List<Assets_Opf_Identifiers> identifier { get; set; }    // 1803606/4000000
        [BsonIgnoreIfNull] public List<string> language { get; set; }   // 1803606/4000000
        [BsonIgnoreIfNull] public string title { get; set; }    // 1803606/4000000
        [BsonIgnoreIfNull] public string date { get; set; } // 414362/4000000
        [BsonIgnoreIfNull] public string publisher { get; set; }    // 229536/4000000
        [BsonIgnoreIfNull] public string description { get; set; }  // 202500/4000000
        [BsonIgnoreIfNull] public List<Assets_Opf_Metadata> meta { get; set; }  // 158004/4000000
        [BsonIgnoreIfNull] public List<string> subject { get; set; }    // 152820/4000000
        [BsonIgnoreIfNull] public string rights { get; set; }   // 96156/4000000
        [BsonIgnoreIfNull] public Assets_Opf_Body Body { get; set; }    // 12772/4000000
    }
    public class Assets_FileTypes
    {
        [BsonRequired] public string Encoding { get; set; } // 3944532/4000000
        [BsonRequired] public string FullType { get; set; } // 3944532/4000000
        [BsonRequired] public string MimeType { get; set; } // 3944532/4000000
        [BsonRequired] public string MimeFull { get; set; } // 3944488/4000000
        [BsonIgnoreIfNull] public string Class { get; set; }    // 3454572/4000000
        [BsonIgnoreIfNull] public string Description { get; set; }  // 3454572/4000000
        [BsonIgnoreIfNull] public string File { get; set; } // 3400516/4000000
        [BsonIgnoreIfNull] public string Match { get; set; }    // 2344942/4000000
        [BsonIgnoreIfNull] public string Container { get; set; }    // 50490/4000000
        [BsonIgnoreIfNull] public string Type { get; set; } // 45212/4000000
        [BsonIgnoreIfNull] public string LibGen { get; set; }   // 34084/4000000
        [BsonIgnoreIfNull] public string IAFormat { get; set; } // 14316/4000000
    }
    public class Assets_Covers2_Front
    {
        [BsonIgnoreIfNull] public string Asset { get; set; }    // 29600/4000000
    }
    public class Assets_Meta_PdfInfo
    {
        [BsonIgnoreIfNull] public string map { get; set; }  // 3154033/4000000
    }
    public class Assets_Opf_Body_Packages
    {
        [BsonIgnoreIfNull] public Assets_Opf_Body_Manifests manifest { get; set; }    // 12772/4000000
        [BsonIgnoreIfNull] public Assets_Opf_Body_MetaData metadata { get; set; }    // 12772/4000000
        [BsonIgnoreIfNull] public Asset_Opf_Body_Guides guide { get; set; }   // 12770/4000000
        [BsonIgnoreIfNull] public BsonDocument spine { get; set; }  // 12770/4000000
    }
    public class Assets_Opf_Metadata
    {
        [BsonIgnoreIfNull] public string content { get; set; }  // 4/4000000
        [BsonIgnoreIfNull] public string name { get; set; } // 4/4000000
    }
    public class Assets_Meta_MediaInfo_Files
    {
        [BsonIgnoreIfNull] public Assets_Meta_MediaInfo_FilesTracks track { get; set; }   // 129712/4000000
    }
    public class Assets_Containers
    {
        [BsonIgnoreIfNull] public UInt32? Outcome { get; set; } // 126128/4000000
        [BsonIgnoreIfNull] public string Type { get; set; } // 126128/4000000
        [BsonIgnoreIfNull] public string TypeDetail { get; set; }   // 126128/4000000
        [BsonIgnoreIfNull] public string NARP { get; set; } // 126126/4000000
        [BsonIgnoreIfNull] public string Directory { get; set; }    // 126076/4000000
        [BsonIgnoreIfNull] public bool? Purged { get; set; }    // 80662/4000000
        [BsonIgnoreIfNull] public DateTime? ExtractDate { get; set; }   // 64036/4000000
        [BsonIgnoreIfNull] public string Stack { get; set; }    // 28902/4000000
        [BsonIgnoreIfNull] public string StackId { get; set; }  // 28902/4000000
        [BsonIgnoreIfNull] public List<Byte[]> Stderr { get; set; } // 1256/4000000
        [BsonIgnoreIfNull] public UInt32? SalvageVer { get; set; }  // 1204/4000000
        [BsonIgnoreIfNull] public bool? Salvaged { get; set; }  // 1204/4000000
        [BsonIgnoreIfNull] public bool? Salvage { get; set; }   // 52/4000000
    }
    public class Assets_OLIdentifiers
    {
        [BsonIgnoreIfNull] public List<string> goodreads { get; set; }  // 32006/4000000
        [BsonIgnoreIfNull] public List<string> librarything { get; set; }   // 31268/4000000
        [BsonIgnoreIfNull] public List<string> overdrive { get; set; }  // 4714/4000000
        [BsonIgnoreIfNull] public List<string> google { get; set; } // 46/4000000
        [BsonIgnoreIfNull] public List<string> amazon { get; set; } // 40/4000000
        [BsonIgnoreIfNull] public List<string> isfdb { get; set; }  // 10/4000000
        [BsonIgnoreIfNull] public List<string> libris { get; set; } // 4/4000000
        [BsonIgnoreIfNull] public List<string> bcid { get; set; }   // 2/4000000
        [BsonIgnoreIfNull] public List<string> shelfari { get; set; }   // 2/4000000
        [BsonIgnoreIfNull] public List<string> smashwords_book_download { get; set; }   // 2/4000000
    }
    public class Assets_Opf_Contributors
    {
        [BsonIgnoreIfNull] [BsonElement("file-as")] public string FileAs { get; set; }    // 1803606/4000000
        [BsonIgnoreIfNull] public string role { get; set; } // 1803606/4000000
        [BsonIgnoreIfNull] public string value { get; set; }    // 1803606/4000000
    }
    public class Assets_Baked
    {
        [BsonIgnoreIfNull] public string Oven { get; set; } // 3307742/4000000
        [BsonIgnoreIfNull] public double? OvenLoadTime { get; set; }    // 347592/4000000
        [BsonIgnoreIfNull] public bool? Annealed { get; set; }  // 38/4000000
        [BsonIgnoreIfNull] public UInt32? Block { get; set; }   // 38/4000000
        [BsonIgnoreIfNull] public double? CTime { get; set; }   // 38/4000000
        [BsonIgnoreIfNull] public string Comp { get; set; } // 38/4000000
        [BsonIgnoreIfNull] public UInt32? FileLength { get; set; }  // 38/4000000
        [BsonIgnoreIfNull] public UInt32? Offset { get; set; }  // 38/4000000
        [BsonIgnoreIfNull] public string Part { get; set; } // 38/4000000
        [BsonIgnoreIfNull] public UInt32? RealLength { get; set; }  // 38/4000000
        [BsonIgnoreIfNull] public string Volume { get; set; }   // 38/4000000
    }
    public class Assets_MediaInfo
    {
        [BsonIgnoreIfNull] public Assets_Media_Files File { get; set; }    // 2/4000000
    }

}
