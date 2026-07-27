using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRIM.SDK;

namespace retention_checker
{
    public class SampleMenuLink : TrimMenuLink
    {
        public override string Description
        {
            get { return "Select this menu item to run the sample link and see the log."; }
        }

        public override int MenuID
        {
            get { return 42; }
        }

        public override string Name
        {
            get { return "Sample Menu String (Show Log)"; }
        }

        public override bool SupportsTagged
        {
            get { return false; }
        }
    }

    public class SampleMenuLinkTagged : TrimMenuLink
    {
        public override string Description
        {
            get { return "Select this menu item to run the tagged sample link and see the log."; }
        }

        public override int MenuID
        {
            get { return 43; }
        }

        public override string Name
        {
            get { return "Sample Tagged Menu String (Show Log Tagged)"; }
        }

        public override bool SupportsTagged
        {
            get { return false; }
        }
    }

    public class retention_checker : ITrimAddIn
    {
        // CM calls this automatically — must NOT throw
        public override string ErrorMessage => string.Empty;

        // SINGLE OBJECT EXECUTE
        public override void ExecuteLink(int cmdId, TrimMainObject forObject, ref bool itemWasChanged)
        {
            if (forObject is Record record)
            {
                record.SetNotes("Testing", NotesUpdateType.AppendWithUserStamp);
                record.Save();
                itemWasChanged = true;
            }
        }

        // TAGGED EXECUTE — must exist even if SupportsTagged = false
        public override void ExecuteLink(int cmdId, TrimMainObjectSearch forTaggedObjects)
        {
            // No-op to prevent CM errors
        }

        public override TrimMenuLink[] GetMenuLinks()
        {
            return new TrimMenuLink[]
            {
                new SampleMenuLink(),
                new SampleMenuLinkTagged()
            };
        }

        public override void Initialise(Database db)
        {
            // No-op
        }

        // Only show menu on Records
        public override bool IsMenuItemEnabled(int cmdId, TrimMainObject forObject)
        {
            return forObject is Record;
        }

        public override void PostDelete(TrimMainObject deletedObject)
        {
            // No-op
        }

        public override void PostSave(TrimMainObject savedObject, bool itemWasJustCreated)
        {
            // No-op
        }

        public override bool PreDelete(TrimMainObject modifiedObject)
        {
            return true; // allow delete
        }

        public override bool PreSave(TrimMainObject modifiedObject)
        {
            return true; // allow save
        }

        public override bool SelectFieldValue(FieldDefinition field, TrimMainObject trimObject, string previousValue)
        {
            return false; // do not override field selection
        }

        public override void Setup(TrimMainObject newObject)
        {
            // No-op
        }

        public override bool SupportsField(FieldDefinition field)
        {
            return false; // no field support
        }

        public override bool VerifyFieldValue(FieldDefinition field, TrimMainObject trimObject, string newValue)
        {
            return true; // accept all values
        }
    }
}