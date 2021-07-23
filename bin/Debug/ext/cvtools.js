/**
 * 执行函数
 */
function change(code) {
	try {
		init();
		code = code.trim();
		// vue字段导出
		if (code.startsWith("[") && code.endsWith("]") && code.indexOf("label")) {
			return vueColumnsExport(code);
		}
		// mybatis控制台日志sql格式化
		if (code.indexOf("Preparing:") > -1 && code.indexOf("Parameters:") > -1) {
			return mybatilsConsoleSQLFormat(code);
		}
	} catch (e) {
		return 'JS异常: ' + e.message + '\n' + code;
	}
}

/**
 * mybatis控制日志格式化
 */
function mybatilsConsoleSQLFormat(code) {
	var sql = sub(code, "Preparing:", "\n").replace("\r", "").trim();
	var param = sub(code, "Parameters:", "\n").replace("\r", "").trim();
	var result = '';
	var paramArray = param.split(',');
	var paramIndex = 0, startIndex = 0, endIndex = 0;
	while ((endIndex = sql.indexOf("?", startIndex)) > -1) {
		result += sql.substring(startIndex, endIndex);
		if (paramArray.length >= paramIndex) {
			var paramStr = paramArray[paramIndex++];
			var p = paramStr.indexOf("(") > -1 ? sub(paramStr, null, "(").trim() : paramStr.trim();
			if (paramStr.indexOf("Integer") > -1 || paramStr.indexOf("Long") > -1 || "null" == paramStr.trim()) {
				result += p;
			} else {
				result += "'" + p + "'";
			}
		}
		startIndex = endIndex + 1;
	}
	return result;
}

/**
 * vue 字段导出
 */
function vueColumnsExport(code) {
	var fIdx = -1;
	while ((fIdx = code.indexOf("formatter:")) > -1) {
		var code1 = code.substring(0, code.substring(0, fIdx).lastIndexOf(","));
		var endIdx = code.indexOf("},", fIdx);
		if (endIdx == -1) {
			code = code1 + "}]";
		} else {
			code = code1 + code.substring(endIdx);
		}
	}
	while ((fIdx = code.indexOf("this.$t(")) > -1) {
		var code1 = code.substring(0, fIdx);
		var endIdx = code.indexOf(")", fIdx);
		code = code1 + code.substring(fIdx + "this.$t(".length, endIdx) + code.substring(endIdx + 1);
	}
	var columns = JSON.parse(code);
	this.$portal = {};
	this.$t = function (t) { return t; };
	var sql = "";
	sql += "set @pool = 'xxxPool';\n";
	sql += "set @exportType = 'xxxExport';\n\n";
	sql += "update data_export_config set is_deleted = 1 where pool = @pool and type = @exportType;\n";
	sql += "insert into data_export_config(pool, type, max_rows, note, company_id) values (@pool, @exportType, 100000, 'xxx导出', -1);\n";
	sql += "select @exportId := id from data_export_config where pool = @pool and type = @exportType and is_deleted = 0 order by id desc limit 1;\n\n";
	sql += "update data_export_config_item set is_deleted = 1 where config_id = @exportId;\n";
	sql += "insert into data_export_config_item(config_id, title, field, sort, align, company_id) values\n";
	var sort = 0;
	for (var k in columns) {
		sort = sort + 1;
		sql += "(@exportId, '"+ columns[k].label + "', '"+ (columns[k].prop || columns[k].slot) +"', "+ sort +", 'center', -1),\n";
	}
	return sql.substring(0, sql.length - 2) + ';';
}

function sub(str, start, end, startIndex) {
	startIndex = startIndex || 0;
	if (str == "") {
		return str;
	}
	var sIndex = start != null ? str.indexOf(start, startIndex) : startIndex;
	if (end == null) {
		return str.substring(sIndex + 1);
	}
	if (sIndex > -1) {
		if (start != null) {
			sIndex += start.length;
		}
		var eIndex = str.indexOf(end, sIndex);
		if (eIndex > 0) {
			return str.substring(sIndex, eIndex);
		}
	}
	return "";
}

function init() {
	// 旧 ie 浏览器不兼容新 js 特效处理
	String.prototype.trim = String.prototype.trim || function() {
		return this.replace(/^\s+|\s+$/g,'');
	};
	String.prototype.startsWith = String.prototype.startsWith || function (str) {
		return this.indexOf(str) == 0;
	}
	String.prototype.endsWith = String.prototype.endsWith || function (str) {
		if (str.length > this.length) {
			return false;
		}
		return this.substring(this.length - str.length) == str;
	}
	if (!this.JSON) {
		this.JSON = {};
		this.JSON.parse = function (s) { return eval("(" + s + ")"); };
		this.JSON.stringify = function() { return '不支持JSON.stringify'; }
	}
}
