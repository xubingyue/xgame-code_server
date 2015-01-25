package com.game.core.msg;

import java.util.ArrayList;
import java.util.List;

import com.game.core.scene.MyScene;
import com.game.core.utils.OutBool;

/**
 * 主消息分派
 * 
 * @author hjj2017
 * @since 2014/5/2
 * 
 */
public class MsgDispatcher {
	/** 单例对象 */
	public static final MsgDispatcher OBJ = new MsgDispatcher();
	/** 消息接收者列表 */
	private List<IMsgReceiver> _msgRecvList = null;

	/**
	 * 添加消息接收者, 
	 * 消息接收者一般是场景对象 {@link MyScene}.
	 * 但是这里使用的是一个接口 {@link IMsgReceiver}, 
	 * 其目的是将消息分派者与具体的消息接收者分离!
	 * 否则, 就需要在消息包里包含场景包, 
	 * 在理论上,
	 * 这是不应该发生的...
	 * 
	 * @param value 消息接收者
	 * @return 
	 * 
	 */
	public MsgDispatcher addMsgReceiver(IMsgReceiver value) {
		if (value == null) {
			// 如果参数对象为空, 
			// 则直接退出!
			return this;
		}

		if (this._msgRecvList == null) {
			// 如果接收者列表为空, 
			// 则创建列表
			this._msgRecvList = new ArrayList<>();
		}

		// 添加接收者
		this._msgRecvList.add(value);
		return this;
	}

	/**
	 * 分派消息
	 * 
	 * @param msgObj 
	 * 
	 */
	public void dispatch(BaseMsg msgObj) {
		if (msgObj == null) {
			// 如果消息对象为空, 
			// 则直接退出!
			return;
		}

		// 是否向下执行
		OutBool nextRecv = new OutBool();
		nextRecv.setVal(true);

		this._msgRecvList.forEach((r) -> {
			// 接收消息
			r.tryReceive(msgObj);
		});
	}
}