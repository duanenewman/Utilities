<Query Kind="Program" />

XNamespace nsXTD = XNamespace.Get("http://schemas.microsoft.com/XML-Document-Transform");

void Main()
{
	var config1 = GetDebugConfig();
	var config2 = GetTargetConfig();
		
	var configOut = new XElement("configuration", new XAttribute(XNamespace.Xmlns + "xdt", nsXTD));
	
	configOut.Add(ProcessNode(config1, config2, "connectionStrings", "add", "name"));
	configOut.Add(ProcessNode(config1, config2, "appSettings", "add", "key"));
	
	configOut.Dump();
}

private XElement ProcessNode(XElement config1, XElement config2, string section, string elementName, string matchAttributeName)
{

	var transformNode = new XElement(section);

	foreach (var node in config2.Elements(section).First().Elements(elementName))
	{

		var node2 = config1.XPathSelectElement(string.Format("/{0}/{1}[@{2}='{3}']", section, elementName, matchAttributeName, node.Attribute(matchAttributeName).Value));
						
		var hasChanges = node2 == null;

		var nodeTransform = new XElement(elementName, 
			new XAttribute(matchAttributeName, node.Attribute(matchAttributeName).Value),
						new XAttribute(nsXTD + "Transform", hasChanges ? "Insert" : "SetAttributes"),
						new XAttribute(nsXTD + "Locator", "Match(" + matchAttributeName + ")"));

		foreach (var nodeAttr in node.Attributes().Where (n => n.Name != matchAttributeName))
		{
		
			nodeTransform.Add(new XAttribute(nodeAttr.Name, nodeAttr.Value));

			if (!hasChanges)
			{
				var node2Attr = node2.Attribute(nodeAttr.Name);
				if (node2Attr == null || node2Attr.Value != nodeAttr.Value)
				{
					hasChanges = true;				
				}
			}
		}
		
		if (hasChanges)
		{
			transformNode.Add(nodeTransform);
		}
	}
	
	return transformNode;
}

// Define other methods and classes here

private XElement GetDebugConfig()
{
	var xml = @"
		<configuration>
			<connectionStrings>
				<add name=""MyConnectionStringName"" connectionString=""My Connection String Original Value"" ProviderType=""Original Provider"" />
				<add name=""MyConnectionStringNameTwo"" connectionString=""My Connection String Two Unchanged Value"" ProviderType=""Original Provider"" />
			</connectionStrings>
			<appSettings>
				<add key=""MyAppSettingOne"" value=""App Setting One Original Value"" />
				<add key=""MyAppSettingTwo"" value=""App Setting Two Un-changed Value"" />
			</appSettings>
		</configuration>";
		
	using (var sr = new StringReader(xml))
	{
		return XElement.Load(sr);
	}

}

private XElement GetTargetConfig()
{
	var xml = @"
		<configuration>
			<connectionStrings>
				<add name=""MyConnectionStringName"" connectionString=""My Connection String New Value"" ProviderType=""Original Provider"" />
				<add name=""MyConnectionStringNameTwo"" connectionString=""My Connection String Two Unchanged Value"" ProviderType=""Original Provider"" />
				<add name=""MyNewConnectionStringName"" connectionString=""My New Connection String Value"" ProviderType=""New Provider"" />
			</connectionStrings>
			<appSettings>
				<add key=""MyAppSettingOne"" value=""App Setting One New Value"" />
				<add key=""MyAppSettingTwo"" value=""App Setting Two Un-changed Value"" />
			</appSettings>
		</configuration>";
		
	using (var sr = new StringReader(xml))
	{
		return XElement.Load(sr);
	}
}