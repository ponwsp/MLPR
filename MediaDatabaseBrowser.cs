using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bosch.VideoSDK.MediaDatabase;

namespace SCW_APP
{ /// <summary>
  /// Specifies current state of media database browser. 
  /// </summary>
    public enum MediaDatabaseBrowserState
    {
        None,
        TracksLoading,
        TracksLoaded,
        RecordsLoading,
        RecordsLoaded
    }

    /// <summary>
    /// Helper class that performs the search of tracks and records on specific media database and collects found information.
    /// To process found items on the fly you should subscribe for collection (ObservableCollection) events.    
    /// </summary>
    public class MediaDatabaseBrowser
    {
        #region Events

        /// <summary>
        /// A delegate type for progress event.
        /// </summary>
        /// <param name="progress">Current activity progress</param>
        public delegate void OnProgressHandler(int progress);

        /// <summary>
        /// Occurs when the progress value is changed
        /// </summary>
        public event OnProgressHandler OnProgress;

        /// <summary>
        /// A delegate type for state changed event.
        /// </summary>
        /// <param name="state">The new state</param>
        /// <param name="description">State description</param>
        public delegate void OnStateChangedHandler(MediaDatabaseBrowserState state, string description);

        /// <summary>
        /// Occurs when the state is changed
        /// </summary>
        public event OnStateChangedHandler OnStateChanged;

        #endregion

        #region Properties

        /// <summary>
        /// Current media database.
        /// </summary>
        public Bosch.VideoSDK.MediaDatabase.MediaDatabase MediaDatabase { get; private set; }

        /// <summary>
        /// Current activity state.
        /// </summary>
        public MediaDatabaseBrowserState State { get; private set; }

        /// <summary>
        /// The collection with found tracks.
        /// </summary>
        public ObservableCollection<Bosch.VideoSDK.MediaDatabase.Track> Tracks { get; private set; }

        /// <summary>
        /// The collection with found records.
        /// </summary>
        public ObservableCollection<Bosch.VideoSDK.MediaDatabase.SearchResult> Records { get; private set; }

        #endregion

        /// <summary>
        /// Default constructor
        /// </summary>
        public MediaDatabaseBrowser()
        {
            Tracks = new ObservableCollection<Track>();
            Records = new ObservableCollection<SearchResult>();
        }

        #region Methods

        /// <summary>
        /// Search tracks on specified media database
        /// </summary>
        /// <param name="mediaDb">The media database instance</param>
        public void SearchTracks(Bosch.VideoSDK.MediaDatabase.MediaDatabase mediaDb)
        {
            if (mediaDb == null)
                throw new ArgumentNullException("mediaDb");

            // reset properties
            MediaDatabase = mediaDb;
            Tracks.Clear();
            Records.Clear();
            SetState(MediaDatabaseBrowserState.TracksLoading, "Loading tracks...");

            // create search session
            var trackSearchSession = MediaDatabase.CreateSearchSession(Bosch.VideoSDK.MediaDatabase.SearchTypeEnum.steTrack);

            // handeler for data callback
            trackSearchSession.TrackAvailable += ((searchType, track, searchSession) =>
            {
                Trace.TraceInformation("Track #{0} found", track.TrackID);

                Tracks.Add(track);
            });

            // handler for progress changes
            trackSearchSession.Progress += ((progress, searchSession) =>
            {
                SetProgress(progress, "Track search progress");

                if (searchSession.IsComplete)
                    SetState(MediaDatabaseBrowserState.TracksLoaded);
            });

            trackSearchSession.Start();
        }

        /// <summary>
        /// Search records on specified track.
        /// </summary>
        /// <param name="trackId">The track's identifier.</param>
        /// <param name="firstAndLastOnly">The option to search first and last items only.</param>
        public void SearchTrackRecords(int trackId, bool firstAndLastOnly = false)
        {
            SetState(MediaDatabaseBrowserState.RecordsLoading, "Loading records for track #" + trackId + "...");

            var recordsSearchSession = MediaDatabase.CreateSearchSession(Bosch.VideoSDK.MediaDatabase.SearchTypeEnum.steEvent);

            recordsSearchSession.FirstAndLastOnly = firstAndLastOnly;

            // search only video records
            recordsSearchSession.AddEventFilter(Bosch.VideoSDK.MediaDatabase.EventTypeEnum.eteVideoRecorded);

            // set filter for specified track ID
            recordsSearchSession.AddIdentifierFilter(trackId);

            // handler for data callback
            recordsSearchSession.ResultAvailable += ((searchType, searchResult, searchSession) =>
            {
                Trace.TraceInformation("Record found on track #{0}: {1} - {2}  {3}",
                    searchResult.TrackID,
                    searchResult.StartTime.UTC,
                    searchResult.EndTime.UTC,
                    searchResult.Text);

                Records.Add(searchResult);
            });

            // handler for progress changes
            recordsSearchSession.Progress += ((progress, searchSession) =>
            {
                SetProgress(progress, "Records search progress");

                if (searchSession.IsComplete)
                    SetState(MediaDatabaseBrowserState.RecordsLoaded);
            });

            recordsSearchSession.Start();
        }

        /// <summary>
        /// Change current state to specified.
        /// </summary>
        /// <param name="state">The new state</param>
        /// <param name="description">The state description</param>
        private void SetState(MediaDatabaseBrowserState state, string description = null)
        {
            Trace.TraceInformation(description);

            State = state;

            if (OnStateChanged != null)
                OnStateChanged(state, description);
        }

        /// <summary>
        /// Chanage current progress to specified value.
        /// </summary>
        /// <param name="progress">The progress value</param>
        /// <param name="description">The current activity description</param>
        private void SetProgress(int progress, string description)
        {
            Trace.TraceInformation("{1} : {0}%", progress, description);

            if (OnProgress != null)
                OnProgress(progress);
        }
        #endregion
    }
}
