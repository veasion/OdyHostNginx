function params() { return "导出|字典|翻译|变量|变量U|转换"; }

/**
 * 执行函数
 */
function change(code, param) {
	try {
		code = code.trim();
		param = (param || "").trim();
		// mybatis控制台日志sql格式化
		if (code.indexOf("Preparing:") > -1 && code.indexOf("Parameters:") > -1) {
			return mybatilsConsoleSQLFormat(code);
		}
		// vue字段导出
		if (param == '导出' && code.startsWith("[") && code.endsWith("]")) {
			return vueColumnsExport(code);
		}
		// 字典SQL, ORDER_STATUS: 1 待发货 2 已支付
		if (param == '字典' && /^[A-Z_]+[:：]+/.test(code)) {
			return miscCodeSQL(code);
		}
		// 翻译
		if (param == '翻译') {
			return fanyiOne(code, true);
		}
		// 变量
		if (param == '变量' || param == '变量U') {
			return varCode(code, param.endsWith('U'));
		}
		// sql => ody-db
		if (param.toUpperCase() == '转换') {
			return sql2ody(code);
		}
		return code;
	} catch (e) {
		return 'JS异常: ' + e.message + '\n' + code;
	}
}

/**
 * mybatis控制日志格式化
 */
function mybatilsConsoleSQLFormat(code) {
	var sql = sub(code, "Preparing:", "\n", 0, true).replace("\r", "").trim();
	var param = sub(code, "Parameters:", "\n", 0, true).replace("\r", "").trim();
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
	if (startIndex < sql.length) {
		result += sql.substring(startIndex);
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
	var columns;
	try {
		columns = JSON.parse(code);
	} catch (e) {
		columns = eval(code);
	}
	this.$portal = {};
	this.$t = function (t) { return t; };
	var sql = "";
	sql += "set @pool = 'xxxPool';\n";
	sql += "set @exportType = 'xxxExport';\n\n";
	sql += "update data_export_config set is_deleted = 1 where pool = @pool and type = @exportType;\n";
	sql += "insert into data_export_config(pool, type, max_rows, note, company_id) values (@pool, @exportType, 100000, 'xxx导出', -1);\n";
	sql += "select @exportId := id from data_export_config where pool = @pool and type = @exportType and is_deleted = 0 order by id desc limit 1;\n\n";
	sql += "update data_export_config_item set is_deleted = 1 where config_id = @exportId;\n";
	sql += "insert into data_export_config_item(config_id, title, field, sort, data_format, align, company_id) values\n";
	var sort = 0;
	for (var k in columns) {
		sort = sort + 1;
		sql += "(@exportId, '"+ columns[k].label + "', '"+ (columns[k].prop || columns[k].slot) +"', "+ sort +", null, 'center', -1),\n";
	}
	return sql.substring(0, sql.length - 2) + ';';
}

/**
 * misc字典
 */
function miscCodeSQL(str) {
	var maoIdx = str.indexOf(/^[A-Z_]+:/.test(str) ? ":" : "：");
	var code = str.substring(0, maoIdx);
	var main = str.substring(maoIdx + 1).trim();
	var arr = main.split(" ");
	var keys = [], kvMap = {}, valueMap = {};
	for (var i = 0; i < arr.length; i++) {
		var key = arr[i];
		if (/^\d+$/.test(key)) {
			while (++i < arr.length && arr[i] != "" && !/^\d+$/.test(arr[i])) {
				kvMap[key] = kvMap[key] ? (kvMap[key] + " " + arr[i]): arr[i];
			}
			if (!kvMap[key]) {
				kvMap[key] = "";
			}
			if (kvMap[key]) {
				valueMap[kvMap[key]] = "";
			}
			keys.push(key);
			i--;
		}
	}
	valueMap = fanyi(valueMap);
	var result = "set @pool = '';\nINSERT INTO misc.code (pool, category, parent_code, code, name, data_type, language, sort, is_deleted, company_id) VALUES \n";
	for (var i = 0; i < keys.length; i++) {
		var key = keys[i];
		result += ("(@pool, '" + code + "', null, '" + key + "', '" + kvMap[key] + "', 'string', 'zh_CN', 0, 0, -1), \n");
		result += ("(@pool, '" + code + "', null, '" + key + "', '" + (valueMap[kvMap[key]] || "") + "', 'string', 'en_US', 0, 0, -1)");
		result += (i >= keys.length - 1 ? ";" : ", \n");
	}
	return result;
}

/**
 * 变量
 */
function varCode(code, upperCase) {
	if (/[\u4e00-\u9fa5]+/i.test(code)) {
		code = fanyiOne(code);
	}
	code = code.replace(/(`|'|")+/g, '').replace(/[\s-,]+/g, '_');
	if (upperCase) {
		return code.toUpperCase();
	}
	code = code.toLowerCase()
		.replace(/\_(\w)/g, function (all, letter) {
			return letter.toUpperCase();
		});
	return code;
}

function sql2ody(code) {
	return JSON.parse(httpRequest("http://lowcode.oudianyun.com/uni-lowcode/api//codeTool/sql2OdyDb?shorthand=true", "POST", code)).data;
}

/**
 * 翻译多个
 */
function fanyi(map) {
	var keys = Object.keys(map);
	var query = "";
	for (var i = 0; i < keys.length; i++) {
		query += keys[i];
		if (i < keys.length - 1) {
			query += " # ";
		}
	}
	var result = fanyiOne(query);
	var arrays = result.split(" # ");
	if (arrays.length >= keys.length) {
		for (var i = 0; i < keys.length; i++) {
			map[keys[i]] = arrays[i];
		}
	}
	return map;
}

/**
 * 翻译
 */
function fanyiOne(query, langMatch) {
	var url = "http://api.fanyi.baidu.com/api/trans/vip/translate?";
	var params = {};
	params["q"] = query;
	if (langMatch && !/.*[\u4e00-\u9fa5]+.*$/.test(query)) {
        params["from"] = "en";
        params["to"] = "zh";
    } else {
        params["from"] = "zh";
        params["to"] = "en";
    }
	params["appid"] = "20181108000231625";
	params["salt"] = new Date().getTime();
	params["sign"] = MD5(params["appid"] + params["q"] + params["salt"] + "rLAv7v55HEAyhWbpn3nM");
	var response = get(url, params);
	var json = JSON.parse(response);
	var result = json["trans_result"];
	if (result == null) {
		throw new Error('翻译失败: ' + response);
	}
	var qResult = query;
	for (var i = 0; i < result.length; i++) {
		if (query == result[i].src) {
			return result[i].dst;
		}
		qResult = result[i].dst;
	}
	return qResult;
}

/**
 * GET请求
 */
function get(url, params) {
	url = url.trim()
	if (params) {
		var keys = Object.keys(params);
		if (keys.length > 0) {
			if (!url.endsWith("?") && !url.endsWith("&")) {
				url = url + "?";
			}
			for (var i = 0; i < keys.length; i++) {
				url = url + encodeURIComponent(keys[i]) + "=" + encodeURIComponent(params[keys[i]]);
				if (i < keys.length - 1) {
					url += "&";
				}
			}
		}
	}
	return httpRequest(url, 'GET');
}

/**
 * 请求
 */
function httpRequest(url, method, body, contentType) {
	/*
	var xmlhttp = null;
	if (window.XMLHttpRequest) {
		xmlhttp = new XMLHttpRequest();
	} else if (window.ActiveXObject) {
		xmlhttp = new ActiveXObject("Microsoft.XMLHTTP");
	}
	method = (method || 'GET').toUpperCase();
	if (method == 'GET') {
		xmlhttp.open(method, url, false);
		xmlhttp.send();
	} else if (method == 'POST') {
		xmlhttp.open(method, url, false);
		// application/json, application/x-www-form-urlencoded
		xmlhttp.setRequestHeader("Content-type", contentType || "application/json");
		xmlhttp.send(body);
	}
	if (xmlhttp.readyState == 4 && xmlhttp.status == 200) {
		return xmlhttp.responseText;
	} else {
		return null;
	}
	*/
	return _request(url, method || 'GET', body, contentType || "application/json");
}

function sub(str, start, end, startIndex, openNoEnd) {
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
		} else if (openNoEnd) {
			return str.substring(sIndex);
		}
	}
	return "";
}

function MD5(string){
	function md5_RotateLeft(lValue,iShiftBits){return(lValue<<iShiftBits)|(lValue>>>(32-iShiftBits))}function md5_AddUnsigned(lX,lY){var lX4,lY4,lX8,lY8,lResult;lX8=(lX&2147483648);lY8=(lY&2147483648);lX4=(lX&1073741824);lY4=(lY&1073741824);lResult=(lX&1073741823)+(lY&1073741823);if(lX4&lY4){return(lResult^2147483648^lX8^lY8)}if(lX4|lY4){if(lResult&1073741824){return(lResult^3221225472^lX8^lY8)}else{return(lResult^1073741824^lX8^lY8)}}else{return(lResult^lX8^lY8)}}function md5_F(x,y,z){return(x&y)|((~x)&z)}function md5_G(x,y,z){return(x&z)|(y&(~z))}function md5_H(x,y,z){return(x^y^z)}function md5_I(x,y,z){return(y^(x|(~z)))}function md5_FF(a,b,c,d,x,s,ac){a=md5_AddUnsigned(a,md5_AddUnsigned(md5_AddUnsigned(md5_F(b,c,d),x),ac));return md5_AddUnsigned(md5_RotateLeft(a,s),b)}function md5_GG(a,b,c,d,x,s,ac){a=md5_AddUnsigned(a,md5_AddUnsigned(md5_AddUnsigned(md5_G(b,c,d),x),ac));return md5_AddUnsigned(md5_RotateLeft(a,s),b)}function md5_HH(a,b,c,d,x,s,ac){a=md5_AddUnsigned(a,md5_AddUnsigned(md5_AddUnsigned(md5_H(b,c,d),x),ac));return md5_AddUnsigned(md5_RotateLeft(a,s),b)}function md5_II(a,b,c,d,x,s,ac){a=md5_AddUnsigned(a,md5_AddUnsigned(md5_AddUnsigned(md5_I(b,c,d),x),ac));return md5_AddUnsigned(md5_RotateLeft(a,s),b)}function md5_ConvertToWordArray(string){var lWordCount;var lMessageLength=string.length;var lNumberOfWords_temp1=lMessageLength+8;var lNumberOfWords_temp2=(lNumberOfWords_temp1-(lNumberOfWords_temp1%64))/64;var lNumberOfWords=(lNumberOfWords_temp2+1)*16;var lWordArray=Array(lNumberOfWords-1);var lBytePosition=0;var lByteCount=0;while(lByteCount<lMessageLength){lWordCount=(lByteCount-(lByteCount%4))/4;lBytePosition=(lByteCount%4)*8;lWordArray[lWordCount]=(lWordArray[lWordCount]|(string.charCodeAt(lByteCount)<<lBytePosition));lByteCount++}lWordCount=(lByteCount-(lByteCount%4))/4;lBytePosition=(lByteCount%4)*8;lWordArray[lWordCount]=lWordArray[lWordCount]|(128<<lBytePosition);lWordArray[lNumberOfWords-2]=lMessageLength<<3;lWordArray[lNumberOfWords-1]=lMessageLength>>>29;return lWordArray}function md5_WordToHex(lValue){var WordToHexValue="",WordToHexValue_temp="",lByte,lCount;for(lCount=0;lCount<=3;lCount++){lByte=(lValue>>>(lCount*8))&255;WordToHexValue_temp="0"+lByte.toString(16);WordToHexValue=WordToHexValue+WordToHexValue_temp.substr(WordToHexValue_temp.length-2,2)}return WordToHexValue}function md5_Utf8Encode(string){string=string.replace(/\r\n/g,"\n");var utftext="";for(var n=0;n<string.length;n++){var c=string.charCodeAt(n);if(c<128){utftext+=String.fromCharCode(c)}else{if((c>127)&&(c<2048)){utftext+=String.fromCharCode((c>>6)|192);utftext+=String.fromCharCode((c&63)|128)}else{utftext+=String.fromCharCode((c>>12)|224);utftext+=String.fromCharCode(((c>>6)&63)|128);utftext+=String.fromCharCode((c&63)|128)}}}return utftext}var x=Array();var k,AA,BB,CC,DD,a,b,c,d;var S11=7,S12=12,S13=17,S14=22;var S21=5,S22=9,S23=14,S24=20;var S31=4,S32=11,S33=16,S34=23;var S41=6,S42=10,S43=15,S44=21;string=md5_Utf8Encode(string);x=md5_ConvertToWordArray(string);a=1732584193;b=4023233417;c=2562383102;d=271733878;for(k=0;k<x.length;k+=16){AA=a;BB=b;CC=c;DD=d;a=md5_FF(a,b,c,d,x[k+0],S11,3614090360);d=md5_FF(d,a,b,c,x[k+1],S12,3905402710);c=md5_FF(c,d,a,b,x[k+2],S13,606105819);b=md5_FF(b,c,d,a,x[k+3],S14,3250441966);a=md5_FF(a,b,c,d,x[k+4],S11,4118548399);d=md5_FF(d,a,b,c,x[k+5],S12,1200080426);c=md5_FF(c,d,a,b,x[k+6],S13,2821735955);b=md5_FF(b,c,d,a,x[k+7],S14,4249261313);a=md5_FF(a,b,c,d,x[k+8],S11,1770035416);d=md5_FF(d,a,b,c,x[k+9],S12,2336552879);c=md5_FF(c,d,a,b,x[k+10],S13,4294925233);b=md5_FF(b,c,d,a,x[k+11],S14,2304563134);a=md5_FF(a,b,c,d,x[k+12],S11,1804603682);d=md5_FF(d,a,b,c,x[k+13],S12,4254626195);c=md5_FF(c,d,a,b,x[k+14],S13,2792965006);b=md5_FF(b,c,d,a,x[k+15],S14,1236535329);a=md5_GG(a,b,c,d,x[k+1],S21,4129170786);d=md5_GG(d,a,b,c,x[k+6],S22,3225465664);c=md5_GG(c,d,a,b,x[k+11],S23,643717713);b=md5_GG(b,c,d,a,x[k+0],S24,3921069994);a=md5_GG(a,b,c,d,x[k+5],S21,3593408605);d=md5_GG(d,a,b,c,x[k+10],S22,38016083);c=md5_GG(c,d,a,b,x[k+15],S23,3634488961);b=md5_GG(b,c,d,a,x[k+4],S24,3889429448);a=md5_GG(a,b,c,d,x[k+9],S21,568446438);d=md5_GG(d,a,b,c,x[k+14],S22,3275163606);c=md5_GG(c,d,a,b,x[k+3],S23,4107603335);b=md5_GG(b,c,d,a,x[k+8],S24,1163531501);a=md5_GG(a,b,c,d,x[k+13],S21,2850285829);d=md5_GG(d,a,b,c,x[k+2],S22,4243563512);c=md5_GG(c,d,a,b,x[k+7],S23,1735328473);b=md5_GG(b,c,d,a,x[k+12],S24,2368359562);a=md5_HH(a,b,c,d,x[k+5],S31,4294588738);d=md5_HH(d,a,b,c,x[k+8],S32,2272392833);c=md5_HH(c,d,a,b,x[k+11],S33,1839030562);b=md5_HH(b,c,d,a,x[k+14],S34,4259657740);a=md5_HH(a,b,c,d,x[k+1],S31,2763975236);d=md5_HH(d,a,b,c,x[k+4],S32,1272893353);c=md5_HH(c,d,a,b,x[k+7],S33,4139469664);b=md5_HH(b,c,d,a,x[k+10],S34,3200236656);a=md5_HH(a,b,c,d,x[k+13],S31,681279174);d=md5_HH(d,a,b,c,x[k+0],S32,3936430074);c=md5_HH(c,d,a,b,x[k+3],S33,3572445317);b=md5_HH(b,c,d,a,x[k+6],S34,76029189);a=md5_HH(a,b,c,d,x[k+9],S31,3654602809);d=md5_HH(d,a,b,c,x[k+12],S32,3873151461);c=md5_HH(c,d,a,b,x[k+15],S33,530742520);
	b=md5_HH(b,c,d,a,x[k+2],S34,3299628645);a=md5_II(a,b,c,d,x[k+0],S41,4096336452);d=md5_II(d,a,b,c,x[k+7],S42,1126891415);c=md5_II(c,d,a,b,x[k+14],S43,2878612391);b=md5_II(b,c,d,a,x[k+5],S44,4237533241);a=md5_II(a,b,c,d,x[k+12],S41,1700485571);d=md5_II(d,a,b,c,x[k+3],S42,2399980690);c=md5_II(c,d,a,b,x[k+10],S43,4293915773);b=md5_II(b,c,d,a,x[k+1],S44,2240044497);a=md5_II(a,b,c,d,x[k+8],S41,1873313359);d=md5_II(d,a,b,c,x[k+15],S42,4264355552);c=md5_II(c,d,a,b,x[k+6],S43,2734768916);b=md5_II(b,c,d,a,x[k+13],S44,1309151649);a=md5_II(a,b,c,d,x[k+4],S41,4149444226);d=md5_II(d,a,b,c,x[k+11],S42,3174756917);c=md5_II(c,d,a,b,x[k+2],S43,718787259);b=md5_II(b,c,d,a,x[k+9],S44,3951481745);a=md5_AddUnsigned(a,AA);b=md5_AddUnsigned(b,BB);c=md5_AddUnsigned(c,CC);d=md5_AddUnsigned(d,DD)}return(md5_WordToHex(a)+md5_WordToHex(b)+md5_WordToHex(c)+md5_WordToHex(d)).toLowerCase()
}
