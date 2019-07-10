namespace AEMG
{
    public class MacroItem
    {
        /// <summary>
        /// Name of the macro
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Path to the macro's template
        /// </summary>
        public string TemplateName { get; set; }

        /// <summary>
        /// Description of the macro
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The type of macro: EXP or AD
        /// </summary>
        public MacroItemType Type { get; set; }

        /// <summary>
        /// Wait time for horror
        /// </summary>
        public string Delay { get; set; }
    }
}
