﻿using System;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;
using FluentSharp;
using FluentSharp.CoreLib;
using System.Web.Script.Serialization;
using Microsoft.Security.Application;

namespace TeamMentor.CoreLib
{
	public class TeamMentor_Article
	{ 
        [XmlAttribute] 
        public int  Metadata_Hash                   { get;set; }                 // Metadata Hash   
        [XmlAttribute] 
        public int  Content_Hash                    { get;set; }                 // Content Hash   

		public TeamMentor_Article_Metadata Metadata { get; set; }
        public TeamMentor_Article_Content  Content  { get; set; }

        public TeamMentor_Article()
        { 
            Metadata = new TeamMentor_Article_Metadata();
            Content = new TeamMentor_Article_Content();

            Metadata.Id = Guid.NewGuid();
        }
    }

	public class TeamMentor_Article_Metadata
	{        
		//Article ID(s)
		public Guid     Id                  { get; set; }		// Article GUID
		public string   Id_History          { get; set; }		// Previous GUIDs (populated when copying Articles) - [Comma Delimited]
        public Guid     Library_Id          { get; set; }		// Library GUID
		//Article Title
		public string   Title               { get; set; }		// Article Title

		//4 main MetaData tags
		public string   Category            { get; set; }		// [Comma Delimited]
		public string   Phase               { get; set; }		// [Comma Delimited]
		public string   Technology	        { get; set; }		// [Comma Delimited]
		public string   Type                { get; set; }		// [Comma Delimited]

				
		//MetaData - Used by TM GUI
		public string   DirectLink          { get; set; }		// To enabled direct URL links to this Article
		public string   Tag			        { get; set; }		// To allow articles to have extra Metadata Items [Comma Delimited]
		public string   Security_Demand     { get; set; }		// To allow articles to make special Security Demands [Comma Delimited]

		//MetaData - NOT Used by TM GUI		
		public string   Author              { get; set; }		// Not Used in GUI
		public string   Priority            { get; set; }		// Not Used in GUI
		public string   Status              { get; set; }		// Not Used in GUI
		public string   Source              { get; set; }		// Not Used in GUI
		public string   License             { get; set; }		// Not Used in GUI
    }

	public class TeamMentor_Article_Content
	{ 
           
                                         		            
		[XmlAttribute] public bool Sanitized            { get; set; }		            // Flag to indicate if the data (for example Html) was sanitized before saving
        [XmlAttribute] public string DataType           { get; set; }		            // Flag to indicate if the data (for example Html) was sanitized before saving

        [XmlElement]   public string Description	    { get; set; }		// Can be used to store a description about the Article Data        
                
        [XmlElement]
        public List<string> Files { get; set; }

        [XmlElement][ScriptIgnore]
        public XmlCDataSection Data { get; set;}

        [XmlIgnore]                                     // used to send receive data from JSON (since the serializer doen't support XmlCDataSection)
        public String Data_Json 
            {
                set {                        
                        Data.Value = value.fixXmlDoubleEncodingIssue();
                    }
                get {
                        return Data.Value.fixXmlDoubleEncodingIssue();                        
                    }
            }

        /*{
            get
            {
                XmlDocument _dummyDoc = new XmlDocument();
                return _dummyDoc.CreateCDataSection(Data_Raw);
            }
            set { Data_Raw = (value != null) ? value.Data : null; }
        } */

        public TeamMentor_Article_Content()
        { 
            Data = new XmlDocument().CreateCDataSection("");
            DataType = "Html";
        }

	}


	//this is the class that can used to import/transform the previous version of guidanceItem files into the new 

	[Serializable, XmlRoot(ElementName = "guidanceItem", Namespace="urn:microsoft:guidanceexplorer:guidanceItem")]	
	public class Guidance_Item_Import
	{
        [XmlAttribute] public string Author			{ get; set; }	
        [XmlAttribute] public string Category		{ get; set; }		
		[XmlAttribute] public string id				{ get; set; }	
		[XmlAttribute] public string id_Original	{ get; set; }	
        [XmlAttribute] public string libraryId	    { get; set; }	
		[XmlAttribute] public string phase			{ get; set; }
	    [XmlAttribute] public string Priority		{ get; set; }	
		[XmlAttribute] public string Rule_Type		{ get; set; }
        [XmlAttribute] public string Status			{ get; set; }
        [XmlAttribute] public string Source			{ get; set; }
        [XmlAttribute] public string Technology		{ get; set; }
	    [XmlAttribute] public string Type			{ get; set; }	
		[XmlAttribute] public string type			{ get; set; }	
		[XmlAttribute] public string Type1			{ get; set; }	
		[XmlAttribute] public string title			{ get; set; }	

        [XmlElement]   public string content		{ get; set; }

		public TeamMentor_Article transform()
		{
			// fix the issue with older SI Library Articles
			if (phase == null)
			{
			    phase	    = Rule_Type;
				Rule_Type   = Type;
			}
            var teamMentor_Article = new TeamMentor_Article
                {
                    Metadata = new TeamMentor_Article_Metadata
                        {
                            Id          = id.guid(),
                            Id_History  = id_Original,
                            Library_Id  = libraryId.guid(),
                            Title       = title,
                            Category    = Category,
                            Phase       = phase,
                            Technology  = Technology,
                            Type        = Rule_Type,
                            Author      = Author,
                            Priority    = Priority,
                            Status      = Status,
                            Source      = Source,

                            DirectLink = "",
                            Tag = "",
                            Security_Demand = "",
                        },
                    Content = new TeamMentor_Article_Content
                        {
                            Sanitized = true,
                            DataType = "Html",
                            Data = {Value = content}
                        }
                };

		    teamMentor_Article.setHashes();
            teamMentor_Article.htmlEncode();            //encode contents
            return teamMentor_Article;
		}
	}

    public static class TeamMentor_Article_ExtensionMethods
    {
        public static TeamMentor_Article setHashes(this TeamMentor_Article article)
        { 
            article.Metadata_Hash = article.Metadata.serialize(false).hash();
            article.Content_Hash  = article.Content.serialize(false).hash();
            return article;
        }


/*        public static guidanceItem transform_into_guidanceItem(this TeamMentor_Article article)
        {
            if (article.isNull())
                return null;
            return new guidanceItem()
                    {
                        id          =   article.Metadata.Id.str(),
                        id_original =   article.Metadata.Id_History,
                        title       =   article.Metadata.Title,

                        Technology  =   article.Metadata.Technology,
                        phase       =   article.Metadata.Phase,
                        Category    =   article.Metadata.Category,
                        Rule_Type   =   article.Metadata.Type,

                        content     =   article.Content.Data.Value
                    };
        }*/

        public static TeamMentor_Article teamMentor_Article(this string pathToXmlFile)
        { 
            var article = pathToXmlFile.load<TeamMentor_Article>().htmlEncode(); 
            return article;
        }

        //this causes  double encoding problems with some properties (like the Title on Html Editor) , but removing it opens up more XSS on other viewers (like the Table)
        public static TeamMentor_Article htmlEncode(this TeamMentor_Article article)
        {
            if (article.isNull())
                return null;
            var metaData = article.Metadata;
            foreach(var prop in metaData.type().properties())
            {
                if (prop.PropertyType == typeof(string))
                {
                    var value = (string)metaData.prop(prop.Name);
                    metaData.prop(prop.Name, Encoder.HtmlEncode(value)); 
                }
            }
            if (TMConfig.Current.TMSecurity.Sanitize_HtmlContent)
            {
                article.Content.Data.Value = Sanitizer.GetSafeHtmlFragment(article.Content.Data.Value);
                article.Content.Sanitized = true;
            }

            return article;
        }

        public static void sanitize(this TeamMentor_Article article)
        {
            if (article.Content.DataType.lower() == "html") // tidy the html
            {
                string cdataContent = article.Content.Data.Value.replace("]]>", "]] >");
                // xmlserialization below will break if there is a ]]>  in the text                

                string tidiedHtml = cdataContent.tidyHtml();

                article.Content.Data.Value = tidiedHtml;

                if (article.serialize(false).inValid())
                    // see if the tidied content can be serialized  and if not use the original data              
                {
                    article.Content.Data.Value = cdataContent;
                }
            }
        }

        //fix double encoding caused by JSON?CDATA/XML transfer of XML data
        public static string fixXmlDoubleEncodingIssue(this string htmlContent)
        { 
            return htmlContent.replace("&amp;", "&"); 
        }
    }

}

