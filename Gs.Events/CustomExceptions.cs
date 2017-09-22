using GalleryServer.Business.Interfaces;
using System;
using System.IO;
using System.Runtime.Serialization;

// ReSharper disable once CheckNamespace
namespace GalleryServer.Events.CustomExceptions
{
    /// <summary>
    ///   The exception that is thrown when a general error occurs in the GalleryServer.Web namespace.
    /// </summary>
    [Serializable]
	public class WebException : Exception
	{
		/// <summary>
		///   The exception that is thrown when a general error occurs in the GalleryServer.Web namespace.
		/// </summary>
		public WebException()
			: base("An error has occurred in the GalleryServer.Web namespace.")
		{
		}

		/// <summary>
		///   The exception that is thrown when a general error occurs in the GalleryServer.Web namespace.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		public WebException(string msg)
			: base(msg)
		{
		}

		/// <summary>
		///   The exception that is thrown when a general error occurs in the GalleryServer.Web namespace.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		/// <param name="innerException">
		///   The exception that is the cause of the current exception. If the
		///   innerException parameter is not a null reference, the current exception is raised in a catch
		///   block that handles the inner exception.
		/// </param>
		public WebException(string msg, Exception innerException)
			: base(msg, innerException)
		{
		}

		/// <summary>
		///   The exception that is thrown when a general error occurs in the GalleryServer.Web namespace.
		/// </summary>
		/// <param name="info">
		///   The System.Runtime.Serialization.SerializationInfo that holds the serialized object
		///   data about the exception being thrown.
		/// </param>
		/// <param name="context">
		///   The System.Runtime.Serialization.StreamingContext that contains contextual
		///   information about the source or destination.
		/// </param>
		protected WebException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}

	/// <summary>
	///   The exception that is thrown when a general error occurs in the GalleryServer.Business namespace.
	/// </summary>
	[Serializable]
	public class BusinessException : Exception
	{
		/// <summary>
		///   The exception that is thrown when a general error occurs in the GalleryServer.Business namespace.
		/// </summary>
		public BusinessException()
			: base("An error has occurred in the GalleryServer.Business namespace.")
		{
		}

		/// <summary>
		///   The exception that is thrown when a general error occurs in the GalleryServer.Business namespace.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		public BusinessException(string msg)
			: base(msg)
		{
		}

		/// <summary>
		///   The exception that is thrown when a general error occurs in the GalleryServer.Business namespace.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		/// <param name="innerException">
		///   The exception that is the cause of the current exception. If the
		///   innerException parameter is not a null reference, the current exception is raised in a catch
		///   block that handles the inner exception.
		/// </param>
		public BusinessException(string msg, Exception innerException)
			: base(msg, innerException)
		{
		}

		/// <summary>
		///   The exception that is thrown when a general error occurs in the GalleryServer.Business namespace.
		/// </summary>
		/// <param name="info">
		///   The System.Runtime.Serialization.SerializationInfo that holds the serialized object
		///   data about the exception being thrown.
		/// </param>
		/// <param name="context">
		///   The System.Runtime.Serialization.StreamingContext that contains contextual
		///   information about the source or destination.
		/// </param>
		protected BusinessException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}

	/// <summary>
	///   The exception that is thrown when a general error occurs in the GalleryServer.Data namespace.
	/// </summary>
	[Serializable]
	public class DataException : Exception
	{
		/// <summary>
		///   The exception that is thrown when a general error occurs in the GalleryServer.Data namespace.
		/// </summary>
		public DataException()
			: base("An error has occurred in the GalleryServer.Data namespace.")
		{
		}

		/// <summary>
		///   The exception that is thrown when a general error occurs in the GalleryServer.Data namespace.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		public DataException(string msg)
			: base(msg)
		{
		}

		/// <summary>
		///   The exception that is thrown when a general error occurs in the GalleryServer.Data namespace.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		/// <param name="innerException">
		///   The exception that is the cause of the current exception. If the
		///   innerException parameter is not a null reference, the current exception is raised in a catch
		///   block that handles the inner exception.
		/// </param>
		public DataException(string msg, Exception innerException)
			: base(msg, innerException)
		{
		}

		/// <summary>
		///   The exception that is thrown when a general error occurs in the GalleryServer.Data namespace.
		/// </summary>
		/// <param name="info">
		///   The System.Runtime.Serialization.SerializationInfo that holds the serialized object
		///   data about the exception being thrown.
		/// </param>
		/// <param name="context">
		///   The System.Runtime.Serialization.StreamingContext that contains contextual
		///   information about the source or destination.
		/// </param>
		protected DataException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}

	/// <summary>
	///   The exception that is thrown when an invalid media object is referenced.
	/// </summary>
	[Serializable]
	public class ApplicationNotInitializedException : Exception
	{
        /// <summary>
        ///   Throws an exception to indicate Gallery Server has not been properly initialized.
        /// </summary>
        public ApplicationNotInitializedException()
			: base("Gallery Server has not been properly initialized. This can happen when the initialization code that must run during application startup does not successfully complete, perhaps due to a failure to connect to the data store or another exception. This error can often be resolved by restarting the application.")
		{
		}

        /// <summary>
        ///   Throws an exception to indicate Gallery Server has not been properly initialized.
        /// </summary>
        /// <param name="msg">A message that describes the error.</param>
        public ApplicationNotInitializedException(string msg)
			: base(msg)
		{
		}

        /// <summary>
        ///   Throws an exception to indicate Gallery Server has not been properly initialized.
        /// </summary>
        /// <param name="msg">A message that describes the error.</param>
        /// <param name="innerException">
        ///   The exception that is the cause of the current exception. If the
        ///   innerException parameter is not a null reference, the current exception is raised in a catch
        ///   block that handles the inner exception.
        /// </param>
        public ApplicationNotInitializedException(string msg, Exception innerException)
			: base(msg, innerException)
		{
		}

		/// <summary>
		///   Throws an exception to indicate Gallery Server has not been properly intialized.
		/// </summary>
		/// <param name="info">
		///   The System.Runtime.Serialization.SerializationInfo that holds the serialized object
		///   data about the exception being thrown.
		/// </param>
		/// <param name="context">
		///   The System.Runtime.Serialization.StreamingContext that contains contextual
		///   information about the source or destination.
		/// </param>
		protected ApplicationNotInitializedException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}

	/// <summary>
	///   The exception that is thrown when a user attempts to perform an action the user does not have authorization to perform.
	/// </summary>
	[Serializable]
	public class GallerySecurityException : Exception
	{
		/// <summary>
		///   Throws an exception when a user attempts to perform an action the user does not have authorization to perform.
		/// </summary>
		public GallerySecurityException()
			: base("You do not have authorization to perform the requested action. This could be because of limited permissions granted to anonymous users or, if you are logged in, you do not belong to a role that authorizes the requested action, or none of the roles to which you belong allow the action for the requested album.")
		{
		}

		/// <summary>
		///   Throws an exception when a user attempts to perform an action the user does not have authorization to perform.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		public GallerySecurityException(string msg)
			: base(msg)
		{
		}

		/// <summary>
		///   Throws an exception when a user attempts to perform an action the user does not have authorization to perform.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		/// <param name="innerException">
		///   The exception that is the cause of the current exception. If the
		///   innerException parameter is not a null reference, the current exception is raised in a catch
		///   block that handles the inner exception.
		/// </param>
		public GallerySecurityException(string msg, Exception innerException)
			: base(msg, innerException)
		{
		}

		/// <summary>
		///   Throws an exception when a user attempts to perform an action the user does not have authorization to perform.
		/// </summary>
		/// <param name="info">
		///   The System.Runtime.Serialization.SerializationInfo that holds the serialized object
		///   data about the exception being thrown.
		/// </param>
		/// <param name="context">
		///   The System.Runtime.Serialization.StreamingContext that contains contextual
		///   information about the source or destination.
		/// </param>
		protected GallerySecurityException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}

	/// <summary>
	///   The exception that is thrown when an invalid gallery is referenced.
	/// </summary>
	[Serializable]
	public class InvalidGalleryException : Exception
	{
		/// <summary>
		///   Throws an exception to indicate an invalid gallery.
		/// </summary>
		public InvalidGalleryException()
			: base("Invalid Gallery: A Gallery ID was omitted or, if specified, does not represent a valid gallery.")
		{
		}

		/// <summary>
		///   Throws an exception to indicate an invalid gallery.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		public InvalidGalleryException(string msg)
			: base(msg)
		{
		}

		/// <summary>
		///   Throws an exception to indicate an invalid gallery.
		/// </summary>
		/// <param name="galleryId">The ID of the gallery that is not valid.</param>
		public InvalidGalleryException(int galleryId)
			: base($"Invalid Gallery ID: {galleryId} does not represent a valid gallery.")
		{
		}

		/// <summary>
		///   Throws an exception to indicate an invalid gallery.
		/// </summary>
		/// <param name="galleryId">The ID of the gallery that is not valid.</param>
		/// <param name="innerException">
		///   The exception that is the cause of the current exception. If the
		///   innerException parameter is not a null reference, the current exception is raised in a catch
		///   block that handles the inner exception.
		/// </param>
		public InvalidGalleryException(int galleryId, Exception innerException)
			: base($"Invalid Gallery ID: {galleryId} does not represent a valid gallery.", innerException)
		{
		}

		/// <summary>
		///   Throws an exception to indicate an invalid gallery.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		/// <param name="innerException">
		///   The exception that is the cause of the current exception. If the
		///   innerException parameter is not a null reference, the current exception is raised in a catch
		///   block that handles the inner exception.
		/// </param>
		public InvalidGalleryException(string msg, Exception innerException)
			: base(msg, innerException)
		{
		}

		/// <summary>
		///   Throws an exception to indicate an invalid gallery.
		/// </summary>
		/// <param name="info">
		///   The System.Runtime.Serialization.SerializationInfo that holds the serialized object
		///   data about the exception being thrown.
		/// </param>
		/// <param name="context">
		///   The System.Runtime.Serialization.StreamingContext that contains contextual
		///   information about the source or destination.
		/// </param>
		protected InvalidGalleryException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}

	/// <summary>
	///   The exception that is thrown when an invalid media object is referenced.
	/// </summary>
	[Serializable]
	public class InvalidMediaObjectException : Exception
	{
		/// <summary>
		///   Throws an exception to indicate an invalid media object.
		/// </summary>
		public InvalidMediaObjectException()
			: base("Invalid Media Object: A media object ID was omitted or, if specified, does not represent a valid media object.")
		{
		}

		/// <summary>
		///   Throws an exception to indicate an invalid media object.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		public InvalidMediaObjectException(string msg)
			: base(msg)
		{
		}

		/// <summary>
		///   Throws an exception to indicate an invalid media object.
		/// </summary>
		/// <param name="mediaObjectId">The ID of the media object that is not valid.</param>
		public InvalidMediaObjectException(int mediaObjectId)
			: base($"Invalid Media Object ID: {mediaObjectId} does not represent a valid media object.")
		{
		}

		/// <summary>
		///   Throws an exception to indicate an invalid media object.
		/// </summary>
		/// <param name="mediaObjectId">The ID of the media object that is not valid.</param>
		/// <param name="innerException">
		///   The exception that is the cause of the current exception. If the
		///   innerException parameter is not a null reference, the current exception is raised in a catch
		///   block that handles the inner exception.
		/// </param>
		public InvalidMediaObjectException(int mediaObjectId, Exception innerException)
			: base($"Invalid Media Object ID: {mediaObjectId} does not represent a valid media object.", innerException)
		{
		}

		/// <summary>
		///   Throws an exception to indicate an invalid media object.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		/// <param name="innerException">
		///   The exception that is the cause of the current exception. If the
		///   innerException parameter is not a null reference, the current exception is raised in a catch
		///   block that handles the inner exception.
		/// </param>
		public InvalidMediaObjectException(string msg, Exception innerException)
			: base(msg, innerException)
		{
		}

		/// <summary>
		///   Throws an exception to indicate an invalid media object.
		/// </summary>
		/// <param name="info">
		///   The System.Runtime.Serialization.SerializationInfo that holds the serialized object
		///   data about the exception being thrown.
		/// </param>
		/// <param name="context">
		///   The System.Runtime.Serialization.StreamingContext that contains contextual
		///   information about the source or destination.
		/// </param>
		protected InvalidMediaObjectException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}

	/// <summary>
	///   The exception that is thrown when an invalid album is referenced.
	/// </summary>
	[Serializable]
	public class InvalidAlbumException : Exception
	{
		[NonSerialized] private readonly int _albumId;

		/// <summary>
		///   Throws an exception to indicate an invalid album.
		/// </summary>
		public InvalidAlbumException()
			: base("Invalid Album: An Album ID was omitted or, if specified, does not represent a valid album.")
		{
		}

		/// <summary>
		///   Throws an exception to indicate an invalid album.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		public InvalidAlbumException(string msg)
			: base(msg)
		{
		}

		/// <summary>
		///   Throws an exception to indicate an invalid album.
		/// </summary>
		/// <param name="albumId">The ID of the album that is not valid.</param>
		public InvalidAlbumException(int albumId)
			: base($"Invalid Album ID: {albumId} does not represent a valid album.")
		{
			_albumId = albumId;
		}

		/// <summary>
		///   Throws an exception to indicate an invalid album.
		/// </summary>
		/// <param name="albumId">The ID of the album that is not valid.</param>
		/// <param name="innerException">
		///   The exception that is the cause of the current exception. If the
		///   innerException parameter is not a null reference, the current exception is raised in a catch
		///   block that handles the inner exception.
		/// </param>
		public InvalidAlbumException(int albumId, Exception innerException)
			: base($"Invalid Album ID: {albumId} does not represent a valid album.", innerException)
		{
			_albumId = albumId;
		}

		/// <summary>
		///   Throws an exception to indicate an invalid album.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		/// <param name="innerException">
		///   The exception that is the cause of the current exception. If the
		///   innerException parameter is not a null reference, the current exception is raised in a catch
		///   block that handles the inner exception.
		/// </param>
		public InvalidAlbumException(string msg, Exception innerException)
			: base(msg, innerException)
		{
		}

		/// <summary>
		///   Throws an exception to indicate an invalid album.
		/// </summary>
		/// <param name="info">
		///   The System.Runtime.Serialization.SerializationInfo that holds the serialized object
		///   data about the exception being thrown.
		/// </param>
		/// <param name="context">
		///   The System.Runtime.Serialization.StreamingContext that contains contextual
		///   information about the source or destination.
		/// </param>
		protected InvalidAlbumException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		/// <summary>
		///   Gets the album ID that is causing the exception.
		/// </summary>
		public int AlbumId => _albumId;
	}

	/// <summary>
	///   The exception that is thrown when the user album feature is enabled but the album ID that is specified for
	///   containing the user albums does not exist.
	/// </summary>
	[Serializable]
	public class CannotDeleteAlbumException : Exception
	{
		/// <summary>
		///   Throws an exception when an album cannot be deleted.
		/// </summary>
		public CannotDeleteAlbumException()
			: base("Cannot delete album")
		{
		}

		/// <summary>
		///   Throws an exception when an album cannot be deleted.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		public CannotDeleteAlbumException(string msg)
			: base(msg)
		{
		}

		/// <summary>
		///   Throws an exception when an album cannot be deleted.
		/// </summary>
		/// <param name="albumId">The ID of the album that cannot be deleted.</param>
		public CannotDeleteAlbumException(int albumId)
			: base($"Cannot delete album: The album {albumId} cannot be deleted.")
		{
		}

		/// <summary>
		///   Throws an exception when an album cannot be deleted.
		/// </summary>
		/// <param name="albumId">The ID of the album that cannot be deleted.</param>
		/// <param name="innerException">
		///   The exception that is the cause of the current exception. If the
		///   innerException parameter is not a null reference, the current exception is raised in a catch
		///   block that handles the inner exception.
		/// </param>
		public CannotDeleteAlbumException(int albumId, Exception innerException)
			: base($"Cannot delete album: The album {albumId} cannot be deleted.", innerException)
		{
		}

		/// <summary>
		///   Throws an exception when an album cannot be deleted.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		/// <param name="innerException">
		///   The exception that is the cause of the current exception. If the
		///   innerException parameter is not a null reference, the current exception is raised in a catch
		///   block that handles the inner exception.
		/// </param>
		public CannotDeleteAlbumException(string msg, Exception innerException)
			: base(msg, innerException)
		{
		}

		/// <summary>
		///   Throws an exception when an album cannot be deleted.
		/// </summary>
		/// <param name="info">
		///   The System.Runtime.Serialization.SerializationInfo that holds the serialized object
		///   data about the exception being thrown.
		/// </param>
		/// <param name="context">
		///   The System.Runtime.Serialization.StreamingContext that contains contextual
		///   information about the source or destination.
		/// </param>
		protected CannotDeleteAlbumException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}

	/// <summary>
	///   The exception that is thrown when the .NET Framework is unable to load an image file into the System.Drawing.Bitmap
	///   class. This is probably because it is corrupted, not an image supported by the .NET Framework, or the server does
	///   not have enough memory to process the image.
	/// </summary>
	[Serializable]
	public class UnsupportedImageTypeException : Exception
	{
		[NonSerialized] private UnsupportedImageTypeExceptionState _state;

		/// <summary>
		///   Throws an exception to indicate the .NET Framework is unable to load an image file into the System.Drawing.Bitmap
		///   class. This is probably because it is corrupted, not an image supported by the .NET Framework, or the server does
		///   not have enough memory to process the image.
		/// </summary>
		public UnsupportedImageTypeException()
			: base("Unsupported image: .NET Core is unable to load an image file. This is probably because it is corrupted, not an image supported by the .NET Core, or the server does not have enough memory to process the image.")
		{
		}

		/// <summary>
		///   Throws an exception to indicate the .NET Framework is unable to load an image file into the System.Drawing.Bitmap
		///   class. This is probably because it is corrupted, not an image supported by the .NET Framework, or the server does
		///   not have enough memory to process the image.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		public UnsupportedImageTypeException(string msg)
			: base(msg)
		{
		}

		/// <summary>
		///   Throws an exception to indicate the .NET Framework is unable to load an image file into the System.Drawing.Bitmap
		///   class. This is probably because it is corrupted, not an image supported by the .NET Framework, or the server does
		///   not have enough memory to process the image.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		/// <param name="innerException">
		///   The exception that is the cause of the current exception. If the
		///   innerException parameter is not a null reference, the current exception is raised in a catch
		///   block that handles the inner exception.
		/// </param>
		public UnsupportedImageTypeException(string msg, Exception innerException)
			: base(msg, innerException)
		{
		}

		/// <summary>
		///   Throws an exception to indicate the .NET Framework is unable to load an image file into the System.Drawing.Bitmap
		///   class. This is probably because it is corrupted, not an image supported by the .NET Framework, or the server does
		///   not have enough memory to process the image.
		/// </summary>
		/// <param name="mediaObject">The media object that contains the unsupported image file.</param>
		public UnsupportedImageTypeException(IGalleryObject mediaObject)
			: base($"Unsupported image: the file {(mediaObject?.Original != null ? mediaObject.Original.FileName : string.Empty)} cannot be loaded into .NET Core. This is probably because it is corrupted, not an image supported by .NET Core, or the server does not have enough memory to process the image.")
		{
			_state.MediaObject = mediaObject;

			// In response to SerializeObjectState, we need to provide any state to serialize with the exception.  In this 
			// case, since our state is already stored in an ISafeSerializationData implementation, we can just provide that.
			SerializeObjectState += (exception, eventArgs) => eventArgs.AddSerializedState(_state);
		}

		/// <summary>
		///   Throws an exception to indicate the .NET Framework is unable to load an image file into the System.Drawing.Bitmap
		///   class. This is probably because it is corrupted, not an image supported by the .NET Framework, or the server does
		///   not have enough memory to process the image.
		/// </summary>
		/// <param name="mediaObject">The media object that contains the unsupported image file.</param>
		/// <param name="innerException">
		///   The exception that is the cause of the current exception. If the
		///   innerException parameter is not a null reference, the current exception is raised in a catch
		///   block that handles the inner exception.
		/// </param>
		public UnsupportedImageTypeException(IGalleryObject mediaObject, Exception innerException)
			: base($"Unsupported image: the file {(mediaObject?.Original != null ? mediaObject.Original.FileName : string.Empty)} cannot be loaded into .NET Core. This is probably because it is corrupted, not an image supported by .NET Core, or the server does not have enough memory to process the image.", innerException)
		{
			_state.MediaObject = mediaObject;

			// In response to SerializeObjectState, we need to provide any state to serialize with the exception.  In this 
			// case, since our state is already stored in an ISafeSerializationData implementation, we can just provide that.
			SerializeObjectState += (exception, eventArgs) => eventArgs.AddSerializedState(_state);
		}

		/// <summary>
		///   Throws an exception to indicate the .NET Framework is unable to load an image file into the System.Drawing.Bitmap
		///   class. This is probably because it is corrupted, not an image supported by the .NET Framework, or the server does
		///   not have enough memory to process the image.
		/// </summary>
		/// <param name="info">
		///   The System.Runtime.Serialization.SerializationInfo that holds the serialized object
		///   data about the exception being thrown.
		/// </param>
		/// <param name="context">
		///   The System.Runtime.Serialization.StreamingContext that contains contextual
		///   information about the source or destination.
		/// </param>
		protected UnsupportedImageTypeException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		/// <summary>
		///   Gets the media object that is causing the exception.
		/// </summary>
		public IGalleryObject MediaObject => _state.MediaObject;

	    /// <summary>
		///   Stores any custom state for this exception and enables the serialization of this state.
		/// </summary>
		[Serializable]
		private struct UnsupportedImageTypeExceptionState : ISafeSerializationData
		{
			/// <summary>
			///   Gets the media object that is causing the exception.
			/// </summary>
			public IGalleryObject MediaObject
			{
			    get;
			    set;
			}

			/// <summary>
			///   Completes the deserialization.
			/// </summary>
			/// <param name="obj">The obj.</param>
			void ISafeSerializationData.CompleteDeserialization(object obj)
			{
				// This method is called when deserialization of the exception is complete.
				// Since the exception simply contains an instance of the exception state object, we can repopulate it 
				// here by just setting its instance field to be equal to this deserialized state instance.
				var exception = (UnsupportedImageTypeException)obj;
				exception._state = this;
			}
		}
	}

	/// <summary>
	///   The exception that is thrown when Gallery Server encounters a file it does not recognize as
	///   a valid media object (e.g. image, video, audio, etc.). This may be because the file is a type that
	///   is disabled, or it may have an unrecognized file extension and the allowUnspecifiedMimeTypes configuration
	///   setting is false.
	/// </summary>
	[Serializable]
	public class UnsupportedMediaObjectTypeException : Exception
	{
		[NonSerialized] private UnsupportedMediaObjectTypeExceptionState _state;

		/// <summary>
		///   Throws an exception to indicate a file that is not recognized as a valid media object supported by
		///   Gallery Server. This may be because the file is a type that is disabled, or it may have an
		///   unrecognized file extension and the allowUnspecifiedMimeTypes configuration setting is false.
		/// </summary>
		public UnsupportedMediaObjectTypeException()
			: base("The gallery does not allow files of this type to be added to the gallery.")
		{
		}

		/// <summary>
		///   Throws an exception to indicate a file that is not recognized as a valid media object supported by
		///   Gallery Server. This may be because the file is a type that is disabled, or it may have an
		///   unrecognized file extension and the allowUnspecifiedMimeTypes configuration setting is false.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		/// <param name="innerException">
		///   The exception that is the cause of the current exception. If the
		///   innerException parameter is not a null reference, the current exception is raised in a catch
		///   block that handles the inner exception.
		/// </param>
		public UnsupportedMediaObjectTypeException(string msg, Exception innerException)
			: base(msg, innerException)
		{
		}

		/// <summary>
		///   Throws an exception to indicate a file that is not recognized as a valid media object supported by
		///   Gallery Server. This may be because the file is a type that is disabled, or it may have an
		///   unrecognized file extension and the allowUnspecifiedMimeTypes configuration setting is false.
		/// </summary>
		/// <param name="filePath">
		///   The full filepath to the file that is not recognized as a valid media object
		///   (ex: C:\inetpub\wwwroot\gs\mediaobjects\myvacation\utah\bikingslickrock.jpg).
		/// </param>
		public UnsupportedMediaObjectTypeException(string filePath)
			: base($"Files of this type ({Path.GetExtension(filePath)}) are not allowed to be added to the gallery.")
		{
			_state.MediaObjectFilePath = filePath;

			// In response to SerializeObjectState, we need to provide any state to serialize with the exception.  In this 
			// case, since our state is already stored in an ISafeSerializationData implementation, we can just provide that.
			SerializeObjectState += (exception, eventArgs) => eventArgs.AddSerializedState(_state);
		}

		/// <summary>
		///   Throws an exception to indicate a file that is not recognized as a valid media object supported by
		///   Gallery Server. This may be because the file is a type that is disabled, or it may have an
		///   unrecognized file extension and the allowUnspecifiedMimeTypes configuration setting is false.
		/// </summary>
		/// <param name="file">The FileInfo object that is not recognized as a valid media object.</param>
		public UnsupportedMediaObjectTypeException(FileSystemInfo file)
			: base($"Files of this type ({(file != null ? Path.GetExtension(file.FullName) : String.Empty)}) are not allowed to be added to the gallery.")
		{
			_state.MediaObjectFilePath = (file == null ? "<no file specified>" : file.FullName);

            // In response to SerializeObjectState, we need to provide any state to serialize with the exception.  In this 
            // case, since our state is already stored in an ISafeSerializationData implementation, we can just provide that.
            SerializeObjectState += (exception, eventArgs) => eventArgs.AddSerializedState(_state);
		}

		/// <summary>
		///   Throws an exception to indicate a file that is not recognized as a valid media object supported by
		///   Gallery Server. This may be because the file is a type that is disabled, or it may have an
		///   unrecognized file extension and the allowUnspecifiedMimeTypes configuration setting is false.
		/// </summary>
		/// <param name="file">The FileInfo object that is not recognized as a valid media object.</param>
		/// <param name="innerException">
		///   The exception that is the cause of the current exception. If the
		///   innerException parameter is not a null reference, the current exception is raised in a catch
		///   block that handles the inner exception.
		/// </param>
		public UnsupportedMediaObjectTypeException(FileSystemInfo file, Exception innerException)
			: base($"Files of this type ({(file != null ? Path.GetExtension(file.FullName) : String.Empty)}) are not allowed to be added to the gallery.", innerException)
		{
			_state.MediaObjectFilePath = (file == null ? "<no file specified>" : file.FullName);

            // In response to SerializeObjectState, we need to provide any state to serialize with the exception.  In this 
            // case, since our state is already stored in an ISafeSerializationData implementation, we can just provide that.
            SerializeObjectState += (exception, eventArgs) => eventArgs.AddSerializedState(_state);
		}

		/// <summary>
		///   Throws an exception to indicate a file that is not recognized as a valid media object supported by
		///   Gallery Server. This may be because the file is a type that is disabled, or it may have an
		///   unrecognized file extension and the allowUnspecifiedMimeTypes configuration setting is false.
		/// </summary>
		/// <param name="info">
		///   The System.Runtime.Serialization.SerializationInfo that holds the serialized object
		///   data about the exception being thrown.
		/// </param>
		/// <param name="context">
		///   The System.Runtime.Serialization.StreamingContext that contains contextual
		///   information about the source or destination.
		/// </param>
		protected UnsupportedMediaObjectTypeException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		/// <summary>
		///   Gets the filename of the media object that is causing the exception. Example:
		///   C:\mypics\vacation\grandcanyon.jpg, grandcanyon.jpg
		/// </summary>
		public string MediaObjectFilePath => _state.MediaObjectFilePath;

	    /// <summary>
		///   Stores any custom state for this exception and enables the serialization of this state.
		/// </summary>
		[Serializable]
		private struct UnsupportedMediaObjectTypeExceptionState : ISafeSerializationData
		{
			/// <summary>
			///   Gets the filename of the media object that is causing the exception. Example:
			///   C:\mypics\vacation\grandcanyon.jpg, grandcanyon.jpg
			/// </summary>
			public string MediaObjectFilePath
			{
			    get;
			    set;
			}

			/// <summary>
			///   Completes the deserialization.
			/// </summary>
			/// <param name="obj">The obj.</param>
			void ISafeSerializationData.CompleteDeserialization(object obj)
			{
				// This method is called when deserialization of the exception is complete.
				// Since the exception simply contains an instance of the exception state object, we can repopulate it 
				// here by just setting its instance field to be equal to this deserialized state instance.
				var exception = (UnsupportedMediaObjectTypeException)obj;
				exception._state = this;
			}
		}
	}

	/// <summary>
	///   The exception that is thrown when Gallery Server cannot find a directory.
	/// </summary>
	[Serializable]
	public class InvalidMediaObjectDirectoryException : Exception
	{
		[NonSerialized] private InvalidMediaObjectDirectoryExceptionState _state;

		/// <summary>
		///   Throws an exception to indicate an invalid media objects directory.
		/// </summary>
		public InvalidMediaObjectDirectoryException()
			: base("Invalid media asset directory: Gallery Server cannot find or does not have permission to access the media object directory. Verify that the setting corresponds to a valid directory and that the web application has read, write and modify permission to this location.")
		{
		}

		/// <summary>
		///   Throws an exception to indicate an invalid media objects directory.
		/// </summary>
		/// <param name="mediaObjectPath">The media object directory that is not valid.</param>
		public InvalidMediaObjectDirectoryException(string mediaObjectPath)
			: base($"Invalid media asset directory: Gallery Server cannot find or does not have permission to access the media asset directory \"{mediaObjectPath}\". Verify that the setting corresponds to a valid directory and that the web application has read, write and modify permission to this location.")
		{
			_state.MediaObjectPath = (mediaObjectPath ?? "<no directory specified>");

			// In response to SerializeObjectState, we need to provide any state to serialize with the exception.  In this 
			// case, since our state is already stored in an ISafeSerializationData implementation, we can just provide that.
			SerializeObjectState += (exception, eventArgs) => eventArgs.AddSerializedState(_state);
		}

		/// <summary>
		///   Throws an exception to indicate an invalid media objects directory.
		/// </summary>
		/// <param name="mediaObjectPath">The media object directory that is not valid.</param>
		/// <param name="innerException">
		///   The exception that is the cause of the current exception. If the
		///   innerException parameter is not a null reference, the current exception is raised in a catch
		///   block that handles the inner exception.
		/// </param>
		public InvalidMediaObjectDirectoryException(string mediaObjectPath, Exception innerException)
			: base($"Invalid media asset directory: Gallery Server cannot find or does not have permission to access the media asset directory \"{mediaObjectPath}\". Verify that the setting corresponds to a valid directory and that the web application has read, write and modify permission to this location.", innerException)
		{
			_state.MediaObjectPath = (mediaObjectPath ?? "<no directory specified>");

			// In response to SerializeObjectState, we need to provide any state to serialize with the exception.  In this 
			// case, since our state is already stored in an ISafeSerializationData implementation, we can just provide that.
			SerializeObjectState += (exception, eventArgs) => eventArgs.AddSerializedState(_state);
		}

		/// <summary>
		///   Throws an exception to indicate an invalid media objects directory.
		/// </summary>
		/// <param name="info">
		///   The System.Runtime.Serialization.SerializationInfo that holds the serialized object
		///   data about the exception being thrown.
		/// </param>
		/// <param name="context">
		///   The System.Runtime.Serialization.StreamingContext that contains contextual
		///   information about the source or destination.
		/// </param>
		protected InvalidMediaObjectDirectoryException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		/// <summary>
		///   Gets the media object directory that cannot be written to. Example: C:\inetput\wwwroot\mediaobjects
		/// </summary>
		public string MediaObjectPath => _state.MediaObjectPath;

	    /// <summary>
		///   Stores any custom state for this exception and enables the serialization of this state.
		/// </summary>
		[Serializable]
		private struct InvalidMediaObjectDirectoryExceptionState : ISafeSerializationData
		{
			/// <summary>
			///   Gets the media object directory that cannot be written to. Example: C:\inetput\wwwroot\mediaobjects
			/// </summary>
			public string MediaObjectPath
			{
			    get;
			    set;
			}

			/// <summary>
			///   Completes the deserialization.
			/// </summary>
			/// <param name="obj">The obj.</param>
			void ISafeSerializationData.CompleteDeserialization(object obj)
			{
				// This method is called when deserialization of the exception is complete.
				// Since the exception simply contains an instance of the exception state object, we can repopulate it 
				// here by just setting its instance field to be equal to this deserialized state instance.
				var exception = (InvalidMediaObjectDirectoryException)obj;
				exception._state = this;
			}
		}
	}

	/// <summary>
	///   The exception that is thrown when Gallery Server is unable to write to, or delete from, a directory.
	/// </summary>
	[Serializable]
	public class CannotWriteToDirectoryException : Exception
	{
		[NonSerialized] private CannotWriteToDirectoryExceptionState _state;

		/// <summary>
		///   Throws an exception when Gallery Server is unable to write to, or delete from, a directory.
		/// </summary>
		public CannotWriteToDirectoryException()
			: base("Gallery Server cannot write to a directory.")
		{
		}

		/// <summary>
		///   Throws an exception when Gallery Server is unable to write to, or delete from, a directory.
		/// </summary>
		/// <param name="directoryPath">The directory that cannot be written to.</param>
		public CannotWriteToDirectoryException(string directoryPath)
			: base($"Gallery Server cannot write to the directory \"{directoryPath}\".")
		{
			_state.DirectoryPath = (directoryPath ?? "<no directory specified>");

			// In response to SerializeObjectState, we need to provide any state to serialize with the exception.  In this 
			// case, since our state is already stored in an ISafeSerializationData implementation, we can just provide that.
			SerializeObjectState += (exception, eventArgs) => eventArgs.AddSerializedState(_state);
		}

		/// <summary>
		///   Throws an exception when Gallery Server is unable to write to, or delete from, a directory.
		/// </summary>
		/// <param name="directoryPath">The directory that cannot be written to.</param>
		/// <param name="innerException">
		///   The exception that is the cause of the current exception. If the
		///   innerException parameter is not a null reference, the current exception is raised in a catch
		///   block that handles the inner exception.
		/// </param>
		public CannotWriteToDirectoryException(string directoryPath, Exception innerException)
			: base($"Gallery Server cannot write to the directory \"{directoryPath}\".", innerException)
		{
			_state.DirectoryPath = (directoryPath ?? "<no directory specified>");

			// In response to SerializeObjectState, we need to provide any state to serialize with the exception.  In this 
			// case, since our state is already stored in an ISafeSerializationData implementation, we can just provide that.
			SerializeObjectState += (exception, eventArgs) => eventArgs.AddSerializedState(_state);
		}

		/// <summary>
		///   Throws an exception when Gallery Server is unable to write to, or delete from, a directory.
		/// </summary>
		/// <param name="info">
		///   The System.Runtime.Serialization.SerializationInfo that holds the serialized object
		///   data about the exception being thrown.
		/// </param>
		/// <param name="context">
		///   The System.Runtime.Serialization.StreamingContext that contains contextual
		///   information about the source or destination.
		/// </param>
		protected CannotWriteToDirectoryException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		/// <summary>
		///   Gets the directory that cannot be written to. Example: C:\inetput\wwwroot\mediaobjects
		/// </summary>
		public string DirectoryPath => _state.DirectoryPath;

	    /// <summary>
		///   Stores any custom state for this exception and enables the serialization of this state.
		/// </summary>
		[Serializable]
		private struct CannotWriteToDirectoryExceptionState : ISafeSerializationData
		{
			/// <summary>
			///   Gets the directory that cannot be written to. Example: C:\inetput\wwwroot\mediaobjects
			/// </summary>
			public string DirectoryPath
			{
			    get;
			    set;
			}

			/// <summary>
			///   Completes the deserialization.
			/// </summary>
			/// <param name="obj">The obj.</param>
			void ISafeSerializationData.CompleteDeserialization(object obj)
			{
				// This method is called when deserialization of the exception is complete.
				// Since the exception simply contains an instance of the exception state object, we can repopulate it 
				// here by just setting its instance field to be equal to this deserialized state instance.
				var exception = (CannotWriteToDirectoryException)obj;
				exception._state = this;
			}
		}
	}

	/// <summary>
	///   The exception that is thrown when Gallery Server is unable to read from a directory.
	/// </summary>
	[Serializable]
	public class CannotReadFromDirectoryException : Exception
	{
		[NonSerialized] private CannotReadFromDirectoryExceptionState _state;

		/// <summary>
		///   Throws an exception when Gallery Server is unable to read from a directory.
		/// </summary>
		public CannotReadFromDirectoryException()
			: base("Gallery Server cannot read from a directory. This may be due to insufficient permissions. Check that the directory exists and that the IIS application pool has read permission for it.")
		{
		}

		/// <summary>
		///   Throws an exception when Gallery Server is unable to read from a directory.
		/// </summary>
		/// <param name="directoryPath">The directory that cannot be read from.</param>
		public CannotReadFromDirectoryException(string directoryPath)
			: base($"Gallery Server cannot read from the directory \"{directoryPath}\".This may be due to insufficient permissions. Check that the directory exists and that the web application has read permission for it.")
		{
			_state.DirectoryPath = (directoryPath ?? "<no directory specified>");

			// In response to SerializeObjectState, we need to provide any state to serialize with the exception.  In this 
			// case, since our state is already stored in an ISafeSerializationData implementation, we can just provide that.
			SerializeObjectState += (exception, eventArgs) => eventArgs.AddSerializedState(_state);
		}

		/// <summary>
		///   Throws an exception when Gallery Server is unable to read from a directory.
		/// </summary>
		/// <param name="directoryPath">The directory that cannot be read from.</param>
		/// <param name="innerException">
		///   The exception that is the cause of the current exception. If the
		///   innerException parameter is not a null reference, the current exception is raised in a catch
		///   block that handles the inner exception.
		/// </param>
		public CannotReadFromDirectoryException(string directoryPath, Exception innerException)
			: base($"Gallery Server cannot read from the directory \"{directoryPath}\".This may be due to insufficient permissions. Check that the directory exists and that the web application has read permission for it.", innerException)
		{
			_state.DirectoryPath = (directoryPath ?? "<no directory specified>");

			// In response to SerializeObjectState, we need to provide any state to serialize with the exception.  In this 
			// case, since our state is already stored in an ISafeSerializationData implementation, we can just provide that.
			SerializeObjectState += (exception, eventArgs) => eventArgs.AddSerializedState(_state);
		}

		/// <summary>
		///   Throws an exception when Gallery Server is unable to read from a directory.
		/// </summary>
		/// <param name="info">
		///   The System.Runtime.Serialization.SerializationInfo that holds the serialized object
		///   data about the exception being thrown.
		/// </param>
		/// <param name="context">
		///   The System.Runtime.Serialization.StreamingContext that contains contextual
		///   information about the source or destination.
		/// </param>
		protected CannotReadFromDirectoryException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		/// <summary>
		///   Gets the directory that cannot be read from. Example: C:\inetpub\wwwroot\mediaobjects
		/// </summary>
		public string DirectoryPath => _state.DirectoryPath;

	    /// <summary>
		///   Stores any custom state for this exception and enables the serialization of this state.
		/// </summary>
		[Serializable]
		private struct CannotReadFromDirectoryExceptionState : ISafeSerializationData
		{
			/// <summary>
			///   Gets the directory that cannot be written to. Example: C:\inetpub\wwwroot\mediaobjects
			/// </summary>
			public string DirectoryPath
			{
			    get;
			    set;
			}

			/// <summary>
			///   Completes the deserialization.
			/// </summary>
			/// <param name="obj">The obj.</param>
			void ISafeSerializationData.CompleteDeserialization(object obj)
			{
				// This method is called when deserialization of the exception is complete.
				// Since the exception simply contains an instance of the exception state object, we can repopulate it 
				// here by just setting its instance field to be equal to this deserialized state instance.
				var exception = (CannotReadFromDirectoryException)obj;
				exception._state = this;
			}
		}
	}

	/// <summary>
	///   The exception that is thrown when an invalid gallery server role is referenced, or one is attempted to be created
	///   with invalid parameters.
	/// </summary>
	[Serializable]
	public class InvalidGalleryServerRoleException : Exception
	{
		/// <summary>
		///   Throws an exception to indicate when an invalid gallery server role is referenced, or one is attempted to be created
		///   with invalid parameters.
		/// </summary>
		public InvalidGalleryServerRoleException()
			: base("Invalid Gallery Server Role: The role does not exist in the data store or one is being created with invalid parameters.")
		{
		}

		/// <summary>
		///   Throws an exception to indicate when an invalid gallery server role is referenced, or one is attempted to be created
		///   with invalid parameters.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		public InvalidGalleryServerRoleException(string msg)
			: base(msg)
		{
		}

		/// <summary>
		///   Throws an exception to indicate when an invalid gallery server role is referenced, or one is attempted to be created
		///   with invalid parameters.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		/// <param name="innerException">
		///   The exception that is the cause of the current exception. If the
		///   innerException parameter is not a null reference, the current exception is raised in a catch
		///   block that handles the inner exception.
		/// </param>
		public InvalidGalleryServerRoleException(string msg, Exception innerException)
			: base(msg, innerException)
		{
		}

		/// <summary>
		///   Throws an exception to indicate when an invalid gallery server role is referenced, or one is attempted to be created
		///   with invalid parameters.
		/// </summary>
		/// <param name="info">
		///   The System.Runtime.Serialization.SerializationInfo that holds the serialized object
		///   data about the exception being thrown.
		/// </param>
		/// <param name="context">
		///   The System.Runtime.Serialization.StreamingContext that contains contextual
		///   information about the source or destination.
		/// </param>
		protected InvalidGalleryServerRoleException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}

	/// <summary>
	///   The exception that is thrown when an invalid user is referenced, or one is attempted to be created
	///   with invalid parameters.
	/// </summary>
	[Serializable]
	public class InvalidUserException : Exception
	{
		/// <summary>
		///   Throws an exception to indicate when an invalid user is referenced, or one is attempted to be created
		///   with invalid parameters.
		/// </summary>
		public InvalidUserException()
			: base("Invalid User: The user does not exist in the data store or one is being created with invalid parameters.")
		{
		}

		/// <summary>
		///   Throws an exception to indicate when an invalid user is referenced, or one is attempted to be created
		///   with invalid parameters.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		public InvalidUserException(string msg)
			: base(msg)
		{
		}

		/// <summary>
		///   Throws an exception to indicate when an invalid user is referenced, or one is attempted to be created
		///   with invalid parameters.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		/// <param name="innerException">
		///   The exception that is the cause of the current exception. If the
		///   innerException parameter is not a null reference, the current exception is raised in a catch
		///   block that handles the inner exception.
		/// </param>
		public InvalidUserException(string msg, Exception innerException)
			: base(msg, innerException)
		{
		}

		/// <summary>
		///   Throws an exception to indicate when an invalid user is referenced, or one is attempted to be created
		///   with invalid parameters.
		/// </summary>
		/// <param name="info">
		///   The System.Runtime.Serialization.SerializationInfo that holds the serialized object
		///   data about the exception being thrown.
		/// </param>
		/// <param name="context">
		///   The System.Runtime.Serialization.StreamingContext that contains contextual
		///   information about the source or destination.
		/// </param>
		protected InvalidUserException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}

	/// <summary>
	///   The exception that is thrown when a user attempts to begin a synchronization when another one is already
	///   in progress.
	/// </summary>
	[Serializable]
	public class SynchronizationInProgressException : Exception
	{
		/// <summary>
		///   Throws an exception to indicate the requested synchronization cannot be started because another one is
		///   in progress.
		/// </summary>
		public SynchronizationInProgressException()
			: base("A synchronization is already in progress.")
		{
		}

		/// <summary>
		///   Throws an exception to indicate the requested synchronization cannot be started because another one is
		///   in progress.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		public SynchronizationInProgressException(string msg)
			: base(msg)
		{
		}

		/// <summary>
		///   Throws an exception to indicate the requested synchronization cannot be started because another one is
		///   in progress.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		/// <param name="innerException">
		///   The exception that is the cause of the current exception. If the
		///   innerException parameter is not a null reference, the current exception is raised in a catch
		///   block that handles the inner exception.
		/// </param>
		public SynchronizationInProgressException(string msg, Exception innerException)
			: base(msg, innerException)
		{
		}

		/// <summary>
		///   Throws an exception to indicate the requested synchronization cannot be started because another one is
		///   in progress.
		/// </summary>
		/// <param name="info">
		///   The System.Runtime.Serialization.SerializationInfo that holds the serialized object
		///   data about the exception being thrown.
		/// </param>
		/// <param name="context">
		///   The System.Runtime.Serialization.StreamingContext that contains contextual
		///   information about the source or destination.
		/// </param>
		protected SynchronizationInProgressException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}

	/// <summary>
	///   The exception that is thrown when a user requests the cancellation of an in-progress synchronization.
	/// </summary>
	[Serializable]
	public class SynchronizationTerminationRequestedException : Exception
	{
        /// <summary>
        ///   Throws an exception to indicate when a user requests the cancellation of an in-progress synchronization.
        /// </summary>
        public SynchronizationTerminationRequestedException()
			: base("The end user has requested the cancellation of a currently executing synchronization. The synchronization has been canceled per the request.")
		{
		}

		/// <summary>
		///   Throws an exception to indicate when a user requests the cancellation of an in-progress synchronization.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		public SynchronizationTerminationRequestedException(string msg)
			: base(msg)
		{
		}

		/// <summary>
		///   Throws an exception to indicate when a user requests the cancellation of an in-progress synchronization.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		/// <param name="innerException">
		///   The exception that is the cause of the current exception. If the
		///   innerException parameter is not a null reference, the current exception is raised in a catch
		///   block that handles the inner exception.
		/// </param>
		public SynchronizationTerminationRequestedException(string msg, Exception innerException)
			: base(msg, innerException)
		{
		}

		/// <summary>
		///   Throws an exception to indicate when a user requests the cancellation of an in-progress synchronization.
		/// </summary>
		/// <param name="info">
		///   The System.Runtime.Serialization.SerializationInfo that holds the serialized object
		///   data about the exception being thrown.
		/// </param>
		/// <param name="context">
		///   The System.Runtime.Serialization.StreamingContext that contains contextual
		///   information about the source or destination.
		/// </param>
		protected SynchronizationTerminationRequestedException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}

	/// <summary>
	///   The exception that is thrown when a user tries to transfer (either by moving or copying)
	///   an album to one of its own directories, whether direct or nested. For example,
	///   a user cannot copy an album from D:\gs_store\folder1 to D:\gs_store\folder1\folder2.
	/// </summary>
	[Serializable]
	public class CannotTransferAlbumToNestedDirectoryException : Exception
	{
		/// <summary>
		///   Throws an exception to indicate the requested move or copy album command cannot take place because the destination
		///   album is a child album of the album we are copying, or is the same album as the one we are copying.
		/// </summary>
		public CannotTransferAlbumToNestedDirectoryException()
			: base("You cannot move or copy an album to itself or to one of its child albums. No assets were transferred.")
		{
		}

		/// <summary>
		///   Throws an exception to indicate the requested move or copy album command cannot take place because the destination
		///   album is a child album of the album we are copying, or is the same album as the one we are copying.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		public CannotTransferAlbumToNestedDirectoryException(string msg)
			: base(msg)
		{
		}

		/// <summary>
		///   Throws an exception to indicate the requested move or copy album command cannot take place because the destination
		///   album is a child album of the album we are copying, or is the same album as the one we are copying.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		/// <param name="innerException">
		///   The exception that is the cause of the current exception. If the
		///   innerException parameter is not a null reference, the current exception is raised in a catch
		///   block that handles the inner exception.
		/// </param>
		public CannotTransferAlbumToNestedDirectoryException(string msg, Exception innerException)
			: base(msg, innerException)
		{
		}

		/// <summary>
		///   Throws an exception to indicate the requested move or copy album command cannot take place because the destination
		///   album is a child album of the album we are copying, or is the same album as the one we are copying.
		/// </summary>
		/// <param name="info">
		///   The System.Runtime.Serialization.SerializationInfo that holds the serialized object
		///   data about the exception being thrown.
		/// </param>
		/// <param name="context">
		///   The System.Runtime.Serialization.StreamingContext that contains contextual
		///   information about the source or destination.
		/// </param>
		protected CannotTransferAlbumToNestedDirectoryException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}

	/// <summary>
	///   The exception that is thrown when a user tries to move a directory but the operating system
	///   won't allow it. This can happen if the user is viewing the contents of the directory in Windows Explorer.
	/// </summary>
	[Serializable]
	public class CannotMoveDirectoryException : Exception
	{
		/// <summary>
		///   Throws an exception to indicate the application is unable to move a directory on the hard drive due to
		///   a restriction by the operating system.
		/// </summary>
		public CannotMoveDirectoryException()
			: base("Cannot move album: The operating system won't allow the directory containing the album to be moved. This can occur when the web server user account has insufficient permissions or the directory contents are being displayed in another window.")
		{
		}

		/// <summary>
		///   Throws an exception to indicate the application is unable to move a directory on the hard drive due to
		///   a restriction by the operating system.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		public CannotMoveDirectoryException(string msg)
			: base(msg)
		{
		}

		/// <summary>
		///   Throws an exception to indicate the application is unable to move a directory on the hard drive due to
		///   a restriction by the operating system.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		/// <param name="innerException">
		///   The exception that is the cause of the current exception. If the
		///   innerException parameter is not a null reference, the current exception is raised in a catch
		///   block that handles the inner exception.
		/// </param>
		public CannotMoveDirectoryException(string msg, Exception innerException)
			: base(msg, innerException)
		{
		}

		/// <summary>
		///   Throws an exception to indicate the application is unable to move a directory on the hard drive due to
		///   a restriction by the operating system.
		/// </summary>
		/// <param name="info">
		///   The System.Runtime.Serialization.SerializationInfo that holds the serialized object
		///   data about the exception being thrown.
		/// </param>
		/// <param name="context">
		///   The System.Runtime.Serialization.StreamingContext that contains contextual
		///   information about the source or destination.
		/// </param>
		protected CannotMoveDirectoryException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}