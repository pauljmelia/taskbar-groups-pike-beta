// This is a generated file! Please edit source .ksy file and use kaitai-struct-compiler to rebuild
// Generated/taken from https://formats.kaitai.io/windows_lnk_file/csharp.html (cc0-1.0)

namespace MadDogs.TaskbarGroups.Editor.Classes
{
    using System.Text;

    using Kaitai;

    /// <summary>
    ///     Windows .lnk files (AKA &quot;shell link&quot; file) are most frequently used
    ///     in Windows shell to create &quot;shortcuts&quot; to another files, usually for
    ///     purposes of running a program from some other directory, sometimes
    ///     with certain preconfigured arguments and some other options.
    /// </summary>
    /// <remarks>
    ///     Reference:
    ///     <a href="https://winprotocoldoc.blob.core.windows.net/productionwindowsarchives/MS-SHLLINK/[MS-SHLLINK].pdf">Source</a>
    /// </remarks>
    public partial class WindowsLnkFile : KaitaiStruct
    {
        public enum DriveTypes
        {
            Unknown = 0,
            NoRootDir = 1,
            Removable = 2,
            Fixed = 3,
            Remote = 4,
            Cdrom = 5,
            Ramdisk = 6
        }

        public enum WindowState
        {
            Normal = 1,
            Maximized = 3,
            MinNoActive = 7
        }

        public WindowsLnkFile(KaitaiStream pIo, KaitaiStruct pParent = null, WindowsLnkFile pRoot = null)
            : base(pIo)
        {
            MParent = pParent;
            MRoot = pRoot ?? this;
            _read();
        }

        public FileHeader Header { get; private set; }

        public LinkTargetIdList TargetIdList { get; private set; }

        public LinkInfo Info { get; private set; }

        public StringData Name { get; private set; }

        public StringData RelPath { get; private set; }

        public StringData WorkDir { get; private set; }

        public StringData Arguments { get; private set; }

        public StringData IconLocation { get; private set; }

        public WindowsLnkFile MRoot { get; }

        public KaitaiStruct MParent { get; }

        public static WindowsLnkFile FromFile(string fileName)
        {
            using (var k = new KaitaiStream(fileName))
            {
                return new WindowsLnkFile(k);
            }
        }

        private void _read()
        {
            Header = new FileHeader(m_io, this, MRoot);

            if (Header.Flags.HasLinkTargetIdList)
            {
                TargetIdList = new LinkTargetIdList(m_io, this, MRoot);
            }

            if (Header.Flags.HasLinkInfo)
            {
                Info = new LinkInfo(m_io, this, MRoot);
            }

            if (Header.Flags.HasName)
            {
                Name = new StringData(m_io, this, MRoot);
            }

            if (Header.Flags.HasRelPath)
            {
                RelPath = new StringData(m_io, this, MRoot);
            }

            if (Header.Flags.HasWorkDir)
            {
                WorkDir = new StringData(m_io, this, MRoot);
            }

            if (Header.Flags.HasArguments)
            {
                Arguments = new StringData(m_io, this, MRoot);
            }

            if (Header.Flags.HasIconLocation)
            {
                IconLocation = new StringData(m_io, this, MRoot);
            }
        }

        /// <remarks>
        ///     Reference:
        ///     <a href="https://winprotocoldoc.blob.core.windows.net/productionwindowsarchives/MS-SHLLINK/[MS-SHLLINK].pdf">
        ///         Section
        ///         2.2
        ///     </a>
        /// </remarks>
        public class LinkTargetIdList : KaitaiStruct
        {
            public LinkTargetIdList(KaitaiStream pIo, WindowsLnkFile pParent = null, WindowsLnkFile pRoot = null)
                : base(pIo)
            {
                MParent = pParent;
                MRoot = pRoot;
                _read();
            }

            public ushort LenIdList { get; private set; }

            public WindowsLnkFile MRoot { get; }

            public WindowsLnkFile MParent { get; }

            public byte[] MRawIdList { get; private set; }

            public static LinkTargetIdList FromFile(string fileName)
            {
                using (var k = new KaitaiStream(fileName))
                {
                    return new LinkTargetIdList(k);
                }
            }

            private void _read()
            {
                LenIdList = m_io.ReadU2le();
                MRawIdList = m_io.ReadBytes(LenIdList);
            }
        }

        public class StringData : KaitaiStruct
        {
            public StringData(KaitaiStream pIo, WindowsLnkFile pParent = null, WindowsLnkFile pRoot = null)
                : base(pIo)
            {
                MParent = pParent;
                MRoot = pRoot;
                _read();
            }

            public ushort CharsStr { get; private set; }

            public string Str { get; private set; }

            public WindowsLnkFile MRoot { get; }

            public WindowsLnkFile MParent { get; }

            public static StringData FromFile(string fileName)
            {
                using (var k = new KaitaiStream(fileName))
                {
                    return new StringData(k);
                }
            }

            private void _read()
            {
                CharsStr = m_io.ReadU2le();

                Str = Encoding.GetEncoding("UTF-16LE")
                              .GetString(m_io.ReadBytes(CharsStr * 2));
            }
        }

        /// <remarks>
        ///     Reference:
        ///     <a href="https://winprotocoldoc.blob.core.windows.net/productionwindowsarchives/MS-SHLLINK/[MS-SHLLINK].pdf">
        ///         Section
        ///         2.3
        ///     </a>
        /// </remarks>
        public class LinkInfo : KaitaiStruct
        {
            private All _all;

            public LinkInfo(KaitaiStream pIo, WindowsLnkFile pParent = null, WindowsLnkFile pRoot = null)
                : base(pIo)
            {
                MParent = pParent;
                MRoot = pRoot;
                _read();
            }

            public uint LenAll { get; private set; }

            public WindowsLnkFile MRoot { get; }

            public WindowsLnkFile MParent { get; }

            public byte[] MRawAll { get; private set; }

            public static LinkInfo FromFile(string fileName)
            {
                using (var k = new KaitaiStream(fileName))
                {
                    return new LinkInfo(k);
                }
            }

            private void _read()
            {
                LenAll = m_io.ReadU4le();
                MRawAll = m_io.ReadBytes(LenAll - 4);

                using (var ioRawAll = new KaitaiStream(MRawAll))
                {
                    _all = new All(ioRawAll, this, MRoot);
                }
            }

            /// <remarks>
            ///     Reference:
            ///     <a href="https://winprotocoldoc.blob.core.windows.net/productionwindowsarchives/MS-SHLLINK/[MS-SHLLINK].pdf">
            ///         Section
            ///         2.3.1
            ///     </a>
            /// </remarks>
            public class VolumeIdBody : KaitaiStruct
            {
                private bool _isUnicode;
                private string _volumeLabelAnsi;
                private bool _fIsUnicode;
                private bool _fVolumeLabelAnsi;

                public VolumeIdBody(KaitaiStream pIo, VolumeIdSpec pParent = null, WindowsLnkFile pRoot = null)
                    : base(pIo)
                {
                    MParent = pParent;
                    MRoot = pRoot;
                    _fIsUnicode = false;
                    _fVolumeLabelAnsi = false;
                    _read();
                }

                public bool IsUnicode
                {
                    get
                    {
                        if (_fIsUnicode)
                        {
                            return _isUnicode;
                        }

                        _isUnicode = OfsVolumeLabel == 20;
                        _fIsUnicode = true;

                        return _isUnicode;
                    }
                }

                public string VolumeLabelAnsi
                {
                    get
                    {
                        if (_fVolumeLabelAnsi)
                        {
                            return _volumeLabelAnsi;
                        }

                        if (!IsUnicode)
                        {
                            var pos = m_io.Pos;
                            m_io.Seek(OfsVolumeLabel - 4);

                            _volumeLabelAnsi = Encoding.GetEncoding("cp437")
                                                       .GetString(m_io.ReadBytesTerm(0, false, true, true));

                            m_io.Seek(pos);
                            _fVolumeLabelAnsi = true;
                        }

                        return _volumeLabelAnsi;
                    }
                }

                public DriveTypes DriveType { get; private set; }

                public uint DriveSerialNumber { get; private set; }

                public uint OfsVolumeLabel { get; private set; }

                public uint? OfsVolumeLabelUnicode { get; private set; }

                public WindowsLnkFile MRoot { get; }

                public VolumeIdSpec MParent { get; }

                public static VolumeIdBody FromFile(string fileName)
                {
                    using (var k = new KaitaiStream(fileName))
                    {
                        return new VolumeIdBody(k);
                    }
                }

                private void _read()
                {
                    DriveType = (DriveTypes)m_io.ReadU4le();
                    DriveSerialNumber = m_io.ReadU4le();
                    OfsVolumeLabel = m_io.ReadU4le();

                    if (IsUnicode)
                    {
                        OfsVolumeLabelUnicode = m_io.ReadU4le();
                    }
                }
            }

            /// <remarks>
            ///     Reference:
            ///     <a href="https://winprotocoldoc.blob.core.windows.net/productionwindowsarchives/MS-SHLLINK/[MS-SHLLINK].pdf">
            ///         Section
            ///         2.3
            ///     </a>
            /// </remarks>
            public class All : KaitaiStruct
            {
                private byte[] _localBasePath;
                private VolumeIdSpec _volumeId;
                private bool _fLocalBasePath;
                private bool _fVolumeId;

                public All(KaitaiStream pIo, LinkInfo pParent = null, WindowsLnkFile pRoot = null)
                    : base(pIo)
                {
                    MParent = pParent;
                    MRoot = pRoot;
                    _fVolumeId = false;
                    _fLocalBasePath = false;
                    _read();
                }

                public VolumeIdSpec VolumeId
                {
                    get
                    {
                        if (_fVolumeId)
                        {
                            return _volumeId;
                        }

                        if (Header.Flags.HasVolumeIdAndLocalBasePath)
                        {
                            var pos = m_io.Pos;
                            m_io.Seek(Header.OfsVolumeId - 4);
                            _volumeId = new VolumeIdSpec(m_io, this, MRoot);
                            m_io.Seek(pos);
                            _fVolumeId = true;
                        }

                        return _volumeId;
                    }
                }

                public byte[] LocalBasePath
                {
                    get
                    {
                        if (_fLocalBasePath)
                        {
                            return _localBasePath;
                        }

                        if (Header.Flags.HasVolumeIdAndLocalBasePath)
                        {
                            var pos = m_io.Pos;
                            m_io.Seek(Header.OfsLocalBasePath - 4);
                            _localBasePath = m_io.ReadBytesTerm(0, false, true, true);
                            m_io.Seek(pos);
                            _fLocalBasePath = true;
                        }

                        return _localBasePath;
                    }
                }

                public uint LenHeader { get; private set; }

                public Header Header { get; private set; }

                public WindowsLnkFile MRoot { get; }

                public LinkInfo MParent { get; }

                public byte[] MRawHeader { get; private set; }

                public static All FromFile(string fileName)
                {
                    using (var k = new KaitaiStream(fileName))
                    {
                        return new All(k);
                    }
                }

                private void _read()
                {
                    LenHeader = m_io.ReadU4le();
                    MRawHeader = m_io.ReadBytes(LenHeader - 8);

                    using (var ioRawHeader = new KaitaiStream(MRawHeader))
                    {
                        Header = new Header(ioRawHeader, this, MRoot);
                    }
                }
            }

            /// <remarks>
            ///     Reference:
            ///     <a href="https://winprotocoldoc.blob.core.windows.net/productionwindowsarchives/MS-SHLLINK/[MS-SHLLINK].pdf">
            ///         Section
            ///         2.3.1
            ///     </a>
            /// </remarks>
            public class VolumeIdSpec : KaitaiStruct
            {
                public VolumeIdSpec(KaitaiStream pIo, All pParent = null, WindowsLnkFile pRoot = null)
                    : base(pIo)
                {
                    MParent = pParent;
                    MRoot = pRoot;
                    _read();
                }

                public uint LenAll { get; private set; }

                public VolumeIdBody Body { get; private set; }

                public WindowsLnkFile MRoot { get; }

                public All MParent { get; }

                public byte[] MRawBody { get; private set; }

                public static VolumeIdSpec FromFile(string fileName)
                {
                    using (var k = new KaitaiStream(fileName))
                    {
                        return new VolumeIdSpec(k);
                    }
                }

                private void _read()
                {
                    LenAll = m_io.ReadU4le();
                    MRawBody = m_io.ReadBytes(LenAll - 4);

                    using (var ioRawBody = new KaitaiStream(MRawBody))
                    {
                        Body = new VolumeIdBody(ioRawBody, this, MRoot);
                    }
                }
            }

            /// <remarks>
            ///     Reference:
            ///     <a href="https://winprotocoldoc.blob.core.windows.net/productionwindowsarchives/MS-SHLLINK/[MS-SHLLINK].pdf">
            ///         Section
            ///         2.3
            ///     </a>
            /// </remarks>
            public class LinkInfoFlags : KaitaiStruct
            {
                public LinkInfoFlags(KaitaiStream pIo, Header pParent = null, WindowsLnkFile pRoot = null)
                    : base(pIo)
                {
                    MParent = pParent;
                    MRoot = pRoot;
                    _read();
                }

                public ulong Reserved1 { get; private set; }

                public bool HasCommonNetRelLink { get; private set; }

                public bool HasVolumeIdAndLocalBasePath { get; private set; }

                public ulong Reserved2 { get; private set; }

                public WindowsLnkFile MRoot { get; }

                public Header MParent { get; }

                public static LinkInfoFlags FromFile(string fileName)
                {
                    using (var k = new KaitaiStream(fileName))
                    {
                        return new LinkInfoFlags(k);
                    }
                }

                private void _read()
                {
                    Reserved1 = m_io.ReadBitsIntBe(6);
                    HasCommonNetRelLink = m_io.ReadBitsIntBe(1) != 0;
                    HasVolumeIdAndLocalBasePath = m_io.ReadBitsIntBe(1) != 0;
                    Reserved2 = m_io.ReadBitsIntBe(24);
                }
            }

            /// <remarks>
            ///     Reference:
            ///     <a href="https://winprotocoldoc.blob.core.windows.net/productionwindowsarchives/MS-SHLLINK/[MS-SHLLINK].pdf">
            ///         Section
            ///         2.3
            ///     </a>
            /// </remarks>
            public class Header : KaitaiStruct
            {
                public Header(KaitaiStream pIo, All pParent = null, WindowsLnkFile pRoot = null)
                    : base(pIo)
                {
                    MParent = pParent;
                    MRoot = pRoot;
                    _read();
                }

                public LinkInfoFlags Flags { get; private set; }

                public uint OfsVolumeId { get; private set; }

                public uint OfsLocalBasePath { get; private set; }

                public uint OfsCommonNetRelLink { get; private set; }

                public uint OfsCommonPathSuffix { get; private set; }

                public uint? OfsLocalBasePathUnicode { get; private set; }

                public uint? OfsCommonPathSuffixUnicode { get; private set; }

                public WindowsLnkFile MRoot { get; }

                public All MParent { get; }

                public static Header FromFile(string fileName)
                {
                    using (var k = new KaitaiStream(fileName))
                    {
                        return new Header(k);
                    }
                }

                private void _read()
                {
                    Flags = new LinkInfoFlags(m_io, this, MRoot);
                    OfsVolumeId = m_io.ReadU4le();
                    OfsLocalBasePath = m_io.ReadU4le();
                    OfsCommonNetRelLink = m_io.ReadU4le();
                    OfsCommonPathSuffix = m_io.ReadU4le();

                    if (!M_Io.IsEof)
                    {
                        OfsLocalBasePathUnicode = m_io.ReadU4le();
                    }

                    if (!M_Io.IsEof)
                    {
                        OfsCommonPathSuffixUnicode = m_io.ReadU4le();
                    }
                }
            }
        }

        /// <remarks>
        ///     Reference:
        ///     <a href="https://winprotocoldoc.blob.core.windows.net/productionwindowsarchives/MS-SHLLINK/[MS-SHLLINK].pdf">
        ///         Section
        ///         2.1.1
        ///     </a>
        /// </remarks>
        public class LinkFlags : KaitaiStruct
        {
            public LinkFlags(KaitaiStream pIo, FileHeader pParent = null, WindowsLnkFile pRoot = null)
                : base(pIo)
            {
                MParent = pParent;
                MRoot = pRoot;
                _read();
            }

            public bool IsUnicode { get; private set; }

            public bool HasIconLocation { get; private set; }

            public bool HasArguments { get; private set; }

            public bool HasWorkDir { get; private set; }

            public bool HasRelPath { get; private set; }

            public bool HasName { get; private set; }

            public bool HasLinkInfo { get; private set; }

            public bool HasLinkTargetIdList { get; private set; }

            public ulong Unnamed8 { get; private set; }

            public ulong Reserved { get; private set; }

            public bool KeepLocalIdListForUncTarget { get; private set; }

            public ulong Unnamed11 { get; private set; }

            public WindowsLnkFile MRoot { get; }

            public FileHeader MParent { get; }

            public static LinkFlags FromFile(string fileName)
            {
                using (var k = new KaitaiStream(fileName))
                {
                    return new LinkFlags(k);
                }
            }

            private void _read()
            {
                IsUnicode = m_io.ReadBitsIntBe(1) != 0;
                HasIconLocation = m_io.ReadBitsIntBe(1) != 0;
                HasArguments = m_io.ReadBitsIntBe(1) != 0;
                HasWorkDir = m_io.ReadBitsIntBe(1) != 0;
                HasRelPath = m_io.ReadBitsIntBe(1) != 0;
                HasName = m_io.ReadBitsIntBe(1) != 0;
                HasLinkInfo = m_io.ReadBitsIntBe(1) != 0;
                HasLinkTargetIdList = m_io.ReadBitsIntBe(1) != 0;
                Unnamed8 = m_io.ReadBitsIntBe(16);
                Reserved = m_io.ReadBitsIntBe(5);
                KeepLocalIdListForUncTarget = m_io.ReadBitsIntBe(1) != 0;
                Unnamed11 = m_io.ReadBitsIntBe(2);
            }
        }

        /// <remarks>
        ///     Reference:
        ///     <a href="https://winprotocoldoc.blob.core.windows.net/productionwindowsarchives/MS-SHLLINK/[MS-SHLLINK].pdf">
        ///         Section
        ///         2.1
        ///     </a>
        /// </remarks>
        public class FileHeader : KaitaiStruct
        {
            public FileHeader(KaitaiStream pIo, WindowsLnkFile pParent = null, WindowsLnkFile pRoot = null)
                : base(pIo)
            {
                MParent = pParent;
                MRoot = pRoot;
                _read();
            }

            /// <summary>
            ///     Technically, a size of the header, but in reality, it's
            ///     fixed by standard.
            /// </summary>
            public byte[] LenHeader { get; private set; }

            /// <summary>
            ///     16-byte class identified (CLSID), reserved for Windows shell
            ///     link files.
            /// </summary>
            public byte[] LinkClsid { get; private set; }

            public LinkFlags Flags { get; private set; }

            public uint FileAttrs { get; private set; }

            public ulong TimeCreation { get; private set; }

            public ulong TimeAccess { get; private set; }

            public ulong TimeWrite { get; private set; }

            /// <summary>
            ///     Lower 32 bits of the size of the file that this link targets
            /// </summary>
            public uint TargetFileSize { get; private set; }

            /// <summary>
            ///     Index of an icon to use from target file
            /// </summary>
            public int IconIndex { get; private set; }

            /// <summary>
            ///     Window state to set after the launch of target executable
            /// </summary>
            public WindowState ShowCommand { get; private set; }

            public ushort Hotkey { get; private set; }

            public byte[] Reserved { get; private set; }

            public WindowsLnkFile MRoot { get; }

            public WindowsLnkFile MParent { get; }

            public byte[] MRawFlags { get; private set; }

            public static FileHeader FromFile(string fileName)
            {
                using (var k = new KaitaiStream(fileName))
                {
                    return new FileHeader(k);
                }
            }

            private void _read()
            {
                LenHeader = m_io.ReadBytes(4);

                if (!(KaitaiStream.ByteArrayCompare(LenHeader, new byte[] { 76, 0, 0, 0 }) == 0))
                {
                    throw new ValidationNotEqualError(new byte[] { 76, 0, 0, 0 },
                                                      LenHeader,
                                                      M_Io,
                                                      "/types/file_header/seq/0");
                }

                LinkClsid = m_io.ReadBytes(16);

                if (!(KaitaiStream.ByteArrayCompare(LinkClsid,
                                                    new byte[] { 1, 20, 2, 0, 0, 0, 0, 0, 192, 0, 0, 0, 0, 0, 0, 70 })
                      == 0))
                {
                    throw new ValidationNotEqualError(new byte[] { 1, 20, 2, 0, 0, 0, 0, 0, 192, 0, 0, 0, 0, 0, 0, 70 },
                                                      LinkClsid,
                                                      M_Io,
                                                      "/types/file_header/seq/1");
                }

                MRawFlags = m_io.ReadBytes(4);

                using (var ioRawFlags = new KaitaiStream(MRawFlags))
                {
                    Flags = new LinkFlags(ioRawFlags, this, MRoot);
                    FileAttrs = m_io.ReadU4le();
                    TimeCreation = m_io.ReadU8le();
                    TimeAccess = m_io.ReadU8le();
                    TimeWrite = m_io.ReadU8le();
                    TargetFileSize = m_io.ReadU4le();
                    IconIndex = m_io.ReadS4le();
                    ShowCommand = (WindowState)m_io.ReadU4le();
                    Hotkey = m_io.ReadU2le();
                    Reserved = m_io.ReadBytes(10);

                    if (!(KaitaiStream.ByteArrayCompare(Reserved, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }) == 0))
                    {
                        throw new ValidationNotEqualError(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                                                          Reserved,
                                                          M_Io,
                                                          "/types/file_header/seq/11");
                    }
                }
            }
        }
    }
}