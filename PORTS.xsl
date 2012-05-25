<?xml version="1.0" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
	<xsl:output method="html" />

	<xsl:template match="/ports">
		<html>
			<head>
				<title>ASAP ports list</title>
				<style>
					table { border-collapse: collapse; }
					th, td { border: solid black 1px; }
					th, .name { background-color: #ccf; }
					.yes { background-color: #cfc; }
					.no { background-color: #fcc; }
					.partial { background-color: #ffc; }
					.yes, .no, .partial { text-align: center; }
				</style>
			</head>
			<body>
				<table>
					<tr>
						<th>Name</th>
						<th>Binary release</th>
						<th>Platform</th>
						<th>User interface</th>
						<th>First appeared in&#160;ASAP</th>
						<th>Develop&#173;ment status</th>
						<th>Output</th>
						<th>Supports subsongs?</th>
						<th>Shows file information?</th>
						<th>Edits file information?</th>
						<th>Converts to and from SAP?</th>
						<th>Configurable playback time?</th>
						<th>Mute POKEY channels?</th>
						<th>Shows STIL?</th>
						<th>Comment</th>
						<th>Program&#173;ming language</th>
						<th>Related website</th>
					</tr>
					<xsl:apply-templates />
				</table>
			</body>
		</html>
	</xsl:template>

	<xsl:template match="port">
		<tr>
			<td class="name"><xsl:value-of select="@name" /></td>
			<td><xsl:value-of select="bin" /></td>
			<td><xsl:value-of select="platform" /></td>
			<td><xsl:value-of select="interface" /></td>
			<td><xsl:value-of select="since" /></td>
			<td>
				<xsl:attribute name="class">
					<xsl:choose>
						<xsl:when test="status = 'stable'">yes</xsl:when>
						<xsl:when test="status = 'experimental'">no</xsl:when>
						<xsl:otherwise>partial</xsl:otherwise>
					</xsl:choose>
				</xsl:attribute>
				<xsl:value-of select="status" />
			</td>
			<td><xsl:value-of select="output" /></td>
			<td><xsl:apply-templates select="subsongs" /></td>
			<td><xsl:apply-templates select="file-info" /></td>
			<td><xsl:apply-templates select="edit-info" /></td>
			<td><xsl:apply-templates select="convert-sap" /></td>
			<td><xsl:apply-templates select="config-time" /></td>
			<td><xsl:apply-templates select="mute-pokey" /></td>
			<td><xsl:apply-templates select="stil" /></td>
			<td><xsl:apply-templates select="comment" /></td>
			<td><xsl:value-of select="lang" /></td>
			<td><xsl:copy-of select="a" /></td>
		</tr>
	</xsl:template>

	<xsl:template match="subsongs|file-info|edit-info|convert-sap|config-time|mute-pokey|stil|comment">
		<xsl:attribute name="class">
			<xsl:choose>
				<xsl:when test="@class"><xsl:value-of select="@class" /></xsl:when>
				<xsl:when test="starts-with(., 'yes')">yes</xsl:when>
				<xsl:when test=". = 'no'">no</xsl:when>
				<xsl:otherwise>partial</xsl:otherwise>
			</xsl:choose>
		</xsl:attribute>
		<xsl:value-of select="." />
	</xsl:template>
</xsl:stylesheet>
