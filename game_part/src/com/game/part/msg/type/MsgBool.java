package com.game.part.msg.type;

import org.apache.mina.core.buffer.IoBuffer;

import com.game.part.msg.IoBuffUtil;

/**
 * 消息中的 Boolean 类型字段
 * 
 * @author hjj2017
 * @since 2015/3/15
 * 
 */
public class MsgBool extends BasicTypeField<Boolean> {
	@Override
	public void readBuff(IoBuffer buff) {
		this.setObjVal(IoBuffUtil.readBool(buff));
	}

	@Override
	public void writeBuff(IoBuffer buff) {
		IoBuffUtil.writeBool(this.getBoolVal(), buff);
	}

	/**
	 * objVal 不能为空, 但如果真为空值, 则自动创建
	 * 
	 * @param objVal
	 * @return
	 * 
	 */
	static MsgBool ifNullThenCreate(MsgBool objVal) {
		if (objVal == null) {
			// 创建对象
			objVal = new MsgBool();
		}

		return objVal;
	}
}
